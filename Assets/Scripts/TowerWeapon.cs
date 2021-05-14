using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType {Cannon = 0, Laser, Slow, Buff,}
public enum WeaponState {SearchTarget = 0, TryAttackCannon, TryAttackLaser,}

public class TowerWeapon : MonoBehaviour
{
    [Header("Commons")]
    [SerializeField]
    private TowerTemplate towerTemplate; // 타워 정보 (공격력, 공격속도 등)
    [SerializeField]
    private Transform spawnPoint; // 발사체 생성 위치
    [SerializeField]
    private WeaponType weaponType; // 무기 속성 결정

    [Header("Cannon")]
    [SerializeField]
    private GameObject projecttilePrefab; // 발사체 프리팹
    private int level = 0; //타워 래벨
    private WeaponState weaponState = WeaponState.SearchTarget; //타워 무기의 상태
    private Transform attackTarget = null; //공격 대상
    private SpriteRenderer spriteRenderer; //타워 오브젝트 이미지 변경용
    private TowerSpawner towerSpawner;
    private EnemySpawner enemySpawner; //게임에 존재하는 적 정보 획득용
    private PlayerGold playerGold; //플레이어의 골드 정보 획득 및 설정
    private Tile ownerTile; //현재 타워가 배치되어 있는 타일

    private float addedDamage; // 버프에 의해 추가된 이미지
    private int buffLevel; // 버프를 받는지 여부 설정 (0 : 버프X, 1~3 : 받는 버프 )

    [Header("Laser")]
    [SerializeField]
    private LineRenderer lineRenderer; // 레이저로 사용되는 선 (LineRenderer)
    [SerializeField]
    private Transform hitEffect; // 타격 효과
    [SerializeField]
    private LayerMask targetLayer; // 광선에 부딪히는 레이어 설정

    public Sprite TowerSprite => towerTemplate.weapon[level].Sprite;
    public float Damage => towerTemplate.weapon[level].Damage;
    public float Rate => towerTemplate.weapon[level].Rate;
    public float Range => towerTemplate.weapon[level].Range;
    public int UpgradeCost => Level < MaxLevel ? towerTemplate.weapon[level + 1].Cost : 0;
    public int SellCost => towerTemplate.weapon[level].Sell;
    public int Level => level + 1;
    public int MaxLevel => towerTemplate.weapon.Length;
    public float Slow => towerTemplate.weapon[level].Slow;
    public float Buff => towerTemplate.weapon[level].Buff;
    public WeaponType WeaponType => weaponType;
    public float AddedDamage
    {
        set => addedDamage = Mathf.Max(0, value);
        get => addedDamage;
    }
    public int BuffLevel
    {
        set => buffLevel = Mathf.Max(0, value);
        get => buffLevel;
    }

    public void Setup(TowerSpawner towerSpawner, EnemySpawner enemySpawner, PlayerGold playerGold, Tile ownerTile)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        this.towerSpawner = towerSpawner;
        this.enemySpawner = enemySpawner;
        this.playerGold = playerGold;
        this.ownerTile = ownerTile;

        //최초 상태를 weaponState.SearchTarget으로 설정
        ChangeState(WeaponState.SearchTarget);

        //무기 속성이 캐논, 레이저 일때
        if(weaponType == WeaponType.Cannon || weaponType == WeaponType.Laser)
        {
            // 최초 상태를 WeaponState.SearchTarget으로 설정
            ChangeState(WeaponState.SearchTarget);
        }
    }

    private void ChangeState(WeaponState newState)
    {
        //이전에 재생중이던 상태종료
        StopCoroutine(weaponState.ToString());
        //상태 변경
        weaponState = newState;
        //새로운 상태 재생
        StartCoroutine(weaponState.ToString());

    }

    void Update()
    {
        if(attackTarget != null)
        {
            RotateToTarget();
        }        
    }

    private void RotateToTarget()
    {
        // 원점으로부터의 거리와 수평출으로부터의 각도를 이용해 위치를 구하는 극 좌표계를 이용
        float dx = attackTarget.position.x - transform.position.x;
        float dy = attackTarget.position.y - transform.position.y;

        // x,y 변위값을 바탕으로 각도 구하기
        // 각도가 radian 단위이기 때문에 Mathf.Rad2Deg를 곱해 도 단위를 구함
        float degree = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, degree);
    }

    private IEnumerator SearchTarget()
    {
        while (true)
        {
            //현재 타워에 가장 가까이 있는 공격 대상 탐색
            attackTarget = FindClosestAttckTarget();

            if(attackTarget != null)
            {
                if(weaponType == WeaponType.Cannon)
                {
                    ChangeState(WeaponState.TryAttackCannon);
                }
                else if(weaponType == WeaponType.Laser)
                {
                    ChangeState(WeaponState.TryAttackLaser);
                }
            }

            yield return null;
        }
    }

    private IEnumerator TryAttackCannon()
    {
        while (true)
        {
            if (!IsPossibleToAttackTarget())
            {
                ChangeState(WeaponState.SearchTarget);
                break;
            }

            //attackRate 시간만큼 대기
            yield return new WaitForSeconds(towerTemplate.weapon[level].Rate);

            //캐논 공격 (발사체 생성)
            SpawnProjecttile();
        }
    }

    private IEnumerator TryAttackLaser()
    {
        //레이저, 레이저 타격효과 활성화
        EnableLaser();

        while (true)
        {
            //target을 공격하는게 가능한지 검사
            if (!IsPossibleToAttackTarget())
            {
                //레이저, 레이저 타격 효과 비활성화
                DisableLaser();
                ChangeState(WeaponState.SearchTarget);
                break;
            }
            //레이저 공격
            SpawnLaser();

            yield return null;
        }
    }

    private void EnableLaser()
    {
        lineRenderer.gameObject.SetActive(true);
        hitEffect.gameObject.SetActive(true);
    }

    private void DisableLaser()
    {
        lineRenderer.gameObject.SetActive(false);
        hitEffect.gameObject.SetActive(false);
    }

    private void SpawnLaser()
    {
        Vector3 direction = attackTarget.position - spawnPoint.position;
        //광선을 여러개 쏘는이유 : 타겟으로 설정한 적과 타워 사이에 다른 적이 끼어 있을 경우 광선은 끼거있는 적에게 부딪혀 멈추기 떄문에
        //타겟의 체력이 달지만 광선과 타격 효과는 다른 적에게 나타난다
        RaycastHit2D[] hit = Physics2D.RaycastAll(spawnPoint.position, direction, towerTemplate.weapon[level].Range, targetLayer);

        //같은 방향으로 여러개의 광선을 쏴서 그중 현재 attackTarget과 동일한 오브젝트 검출
        //attackTarget.position 중심
        //hit[i].point 닿는 가장 가까운 지점
        for(int i = 0; i < hit.Length; i++)
        {
            if(hit[i].transform == attackTarget)
            {
                //선의 시작지점
                lineRenderer.SetPosition(0, spawnPoint.position);
                //선의 목표지점
                lineRenderer.SetPosition(1, new Vector3(hit[i].point.x, hit[i].point.y, 0) + Vector3.back);
                //타격 효과 위치 설정
                hitEffect.position = hit[i].point;
                //적 체력 감소 (1초에 damage만큼 감소)
                //attackTarget.GetComponent<EnemyHP>().TakeDamage(towerTemplate.weapon[level].Damage * Time.deltaTime);
                //공격력 = 타워 기본 공격력 + 버프에 추가된 공격력
                float damage = towerTemplate.weapon[level].Damage + AddedDamage;
                attackTarget.GetComponent<EnemyHP>().TakeDamage(damage * Time.deltaTime);
            }
        }

    }

    public void OnBuffAroundTower()
    {
        // 현재 맵에 배치된 "Tower" 태그를 가진 모든 오브젝트 검색
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");

        for(int i = 0; i < towers.Length; i++)
        {
            TowerWeapon weapon = towers[i].GetComponent<TowerWeapon>();

            //이미 버프를 받고 있고, 현재 버프 타워의 래벨보다 높은 버프이면 패스
            if(weapon.BuffLevel > Level)
            {
                continue;
            }

            //현재 버프 타워와 다른 타워의 거리를 검사해서 범위 안에 타워가 있으면
            if(Vector3.Distance(weapon.transform.position, transform.position) <= towerTemplate.weapon[level].Range)
            {
                //공격이 가능한 캐논, 레이저 타워이면
                if(weapon.WeaponType == WeaponType.Cannon || weapon.WeaponType == WeaponType.Laser)
                {
                    //버프에 의해 공격력 증가
                    weapon.AddedDamage = weapon.Damage * (towerTemplate.weapon[level].Buff);
                    //타워가 받고있는 버프 래벨 설정
                    weapon.BuffLevel = Level;
                }
            }
        }
    }

    private Transform FindClosestAttckTarget()
    {
        // 제일 가까이 있는 적을 찾기 위해 최초 거리를 최대한 크게 설정
        float closestDistSqr = Mathf.Infinity;
        // EnemySpawner의 EnemyList에 있는 현재 맵에 존재하는 모든 적 검사
        for(int i = 0; i < enemySpawner.EnemyList.Count; ++i)
        {
            float distance = Vector3.Distance(enemySpawner.EnemyList[i].transform.position, transform.position);
            //현재 검사중인 적과의 거리가 공격범위 내에 있고, 현재까지 검사한 적보다 거리가 가까우면
            if(distance <= towerTemplate.weapon[level].Range && distance <= closestDistSqr)
            {
                closestDistSqr = distance;
                attackTarget = enemySpawner.EnemyList[i].transform;
            }
        }

        return attackTarget;
    }

    private bool IsPossibleToAttackTarget()
    {
        // target이 있는지 검사 (다른 발사체에 의해 제거, Goal 지점까지 이동해 삭제 등)
        if (attackTarget == null) return false;

        // target이 공격 범위 안에 있는지 검사 (공격 범위를 벗어나면 새로운 적 탐색)
        float distance = Vector3.Distance(attackTarget.position, transform.position);
        if(distance > towerTemplate.weapon[level].Range)
        {
            attackTarget = null;
            return false;
        }
        return true;
    }

    private IEnumerator AttackToTarget()
    {
        while (true)
        {
            // 1. target이 있는지 검사(다른 발사체에 의해 제거, Goal지점 까지 이동해 삭제)
            if(attackTarget == null)
            {
                ChangeState(WeaponState.SearchTarget);
                break;
            }

            // 2.target이 공격 범위 안에 있는지 검사(공격 범위를 벗어나면 새로운 적 탐색)
            float distance = Vector3.Distance(attackTarget.position, transform.position);
            if(distance > towerTemplate.weapon[level].Range)
            {
                attackTarget = null;
                ChangeState(WeaponState.SearchTarget);
                break;
            }

            // 3.attackRate 시간만큼 대기
            yield return new WaitForSeconds(towerTemplate.weapon[level].Rate);

            // 4.공격 (발사체 생성)
            SpawnProjecttile();
        }
    }

    private void SpawnProjecttile()
    {
        GameObject clone = Instantiate(projecttilePrefab, spawnPoint.position, Quaternion.identity);
        //생성한 발사체에게 공격대상(attackTarget) 정보 제공
        //clone.GetComponent<Projecttile>().Setup(attackTarget, towerTemplate.weapon[level].Damage);
        //공격력 = 타워 기본 공격력 + 버프에 의해 추가된 공격력
        float damage = towerTemplate.weapon[level].Damage + AddedDamage;
        clone.GetComponent<Projecttile>().Setup(attackTarget, damage);
    }

    public bool Upgrade()
    {
        if(playerGold.CurrentGold < towerTemplate.weapon[level + 1].Cost)
        {
            return false;
        }
        // 타워 래벨 증가
        level++;
        // 타워 외형 변경(Sprite)
        spriteRenderer.sprite = towerTemplate.weapon[level].Sprite;
        // 골드 차감
        playerGold.CurrentGold -= towerTemplate.weapon[level].Cost;

        //무기 속성 레이저이면
        if(weaponType == WeaponType.Laser)
        {
            //래벨에 따라 레이저의 굵기 설정
            lineRenderer.startWidth = 0.05f + level * 0.05f;
            lineRenderer.endWidth = 0.05f;
        }

        // 타워가 업그레이드 될 때 모든 버프 타워의 버프 효과 갱신
        // 현재 타워가 버프 타워인 경우, 현재 타워가 공격 타워인 경우
        towerSpawner.OnBuffAllBuffTowers();

        return true;
    }

    public void Sell()
    {
        // 골드 증가
        playerGold.CurrentGold += towerTemplate.weapon[level].Sell;
        // 현재 타일에 다시 타워 건설이 가능하도록 설정
        ownerTile.IsBuildTower = false;
        // 타워 파괴
        Destroy(gameObject);
    }


}
