using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSpawner : MonoBehaviour
{
    [SerializeField]
    private TowerTemplate towerTemplate; //타워 정보 (공격력, 공격속도 등)
    //[SerializeField]
    //private GameObject towerPrefab;
    //[SerializeField]
    //private int towerBuildGold = 50; // 타워 건설에 사용되는 골드
    [SerializeField]
    private EnemySpawner enemySpawner; //현재 맵에 존재하는 적 리스트 정보를 얻기 위해
    [SerializeField]
    private PlayerGold playerGold; //타워 건설시 골드 감소를 위해
    [SerializeField]
    private SystemTextViewer systemTextViewer; //돈 부족, 건설 불가와 같은 시스템 메세지 출력

    public void SpawnTower(Transform tileTransform)
    {
        if (towerTemplate.weapon[0].Cost > playerGold.CurrentGold)
        {
            //골드가 부족해서 타워 건설이 불가능 하다고 출력
            systemTextViewer.PrintText(SystemType.Money);
            return;
        }

        Tile tile = tileTransform.GetComponent<Tile>();

        if (tile.IsBuildTower)
        {
            //현재 위치에 이미 타워가 건설되어 있으면 타워 건설 X
            systemTextViewer.PrintText(SystemType.Build);
            return;
        }
        //타워가 건설되어있음으로 설정
        tile.IsBuildTower = true;
        //타워 건설에 필요한 골드만큼 감소
        playerGold.CurrentGold -= towerTemplate.weapon[0].Cost;
        // 선택한 타일의 위치에 타워 건설 (타일보다 z축 -1의 위치에 배치)
        Vector3 position = tileTransform.position + Vector3.back;
        GameObject clone = Instantiate(towerTemplate.towerPrefab, tileTransform.position, Quaternion.identity);
        // 타워 무기에 enemySpawner 정보 전달
        clone.GetComponent<TowerWeapon>().Setup(enemySpawner, playerGold, tile);
    }
}
