using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //[SerializeField]
    //private GameObject enemyPrefab; //적 프리팹
    [SerializeField]
    private GameObject enemyHPSliderPrefab; // 적 체력을 나타내는 Slider UI 프리팹
    [SerializeField]
    private Transform canvasTransform; // UI를 표헌하는 Canva 오브젝트의 Transform
    //[SerializeField]
    //private float spawnTime; //적 생성주기
    [SerializeField]
    private Transform[] wayPoints; //현재 스테이지의 이동 경로
    [SerializeField]
    private PlayerHP playerHP; //플레이어 체력 컴포넌트
    [SerializeField]
    private PlayerGold playerGold; //플레이어 골드 컴포넌트
    private Wave currentWave; //현재 웨이브 정보
    private int currentEnemyCount; // 현재 웨이브에 남아있는 적 숫자 (웨이브 시작시 max로 설정, 적 사망 시 -1)
    private List<Enemy> enemyList; //현재 맵에 존재하는 모든 적의 정보

    //적의 생성과 삭제는 EnemySpawner에서 하기 때문에 Set은 필요 없다
    public List<Enemy> EnemyList => enemyList;

    //현재 웨이브에 남아있는 적, 최대 적 숫자
    public int CurrentEnemyCount => currentEnemyCount;
    public int MaxEnemyCount => currentWave.maxEnemyCount;

    private void Awake()
    {
        //적리스트 메모리 할당
        enemyList = new List<Enemy>();

        //StartCoroutine("SpawnEnemy");
    }

    public void StartWave(Wave wave)
    {
        //매개 변수로 받아온 웨이브 정보 저장
        currentWave = wave;
        //현재 웨이브의 최대 적숫자를 저장
        currentEnemyCount = currentWave.maxEnemyCount;
        //현재 웨이브 시작
        StartCoroutine("SpawnEnemy");
    }

    private IEnumerator SpawnEnemy()
    {
        int spawnEnemyCount = 0;

        while (spawnEnemyCount < currentWave.maxEnemyCount)
        {
            //GameObject clone = Instantiate(enemyPrefab); //적 오브젝트 생성
            //Enemy enemy = clone.GetComponent<Enemy>(); //방금 생성된 적의 Enemy 컴포넌트

            int enemyIndex = UnityEngine.Random.Range(0, currentWave.enemyPrefabs.Length);
            GameObject clone = Instantiate(currentWave.enemyPrefabs[enemyIndex]);
            Enemy enemy = clone.GetComponent<Enemy>(); //방금 생성된 적의 Enemy 컴포넌트


            enemy.Setup(this, wayPoints); //wayPoint정보를 매개변수로 Setup() 호출
            enemyList.Add(enemy);

            SpawnEnemyHPSlider(clone); //적 체력을 나타는 Slider UI 생성 및 설정

            spawnEnemyCount++;

            yield return new WaitForSeconds(currentWave.spawnTime); //spawnTime 시간 동안 대기
        }
    }

    private void SpawnEnemyHPSlider(GameObject enemy)
    {
        // 적 체력을 나타내는 Slider UI 생성
        GameObject sliderClone = Instantiate(enemyHPSliderPrefab);
        // Slider UI 오브젝트를 parent("Canvas" 오브젝트)의 자식으로설정
        // Tip. UI는 캔버스의 자식오브젝트로 설정되어 있어야 화면에 보인다
        sliderClone.transform.SetParent(canvasTransform);

        //계층설정으로 바뀐 크기를 다시 1,1,1 으로 설정
        sliderClone.transform.localScale = Vector3.one;

        // Slider UI가 쫓아다닐 대상을 본인으로 설정
        sliderClone.GetComponent<SliderPositionAutoSetter>().Setup(enemy.transform);

        // Slider UI에 자신의 체력 정보를 표시하도록 설정
        sliderClone.GetComponent<EnemyHPViewer>().Setup(enemy.GetComponent<EnemyHP>());
    }

    public void DestroyEnemy(EnemyDestroyType type, Enemy enemy, int gold)
    {
        if(type == EnemyDestroyType.Arrive)
        {
            // 플레이어의 체력 -1
            playerHP.TakeDamage(1);
        } else if(type == EnemyDestroyType.Kill)
        {
            //적의 종류에 따라 사망시 골드 획득
            playerGold.CurrentGold += gold;
        }

        //적이 사망할 때마다 현재 웨이브의 생존 적 숫자 감소 (UI 표시용)
        currentEnemyCount--;

        //리스트에서 사망하는 적 정보 삭제
        enemyList.Remove(enemy);

        //적 오브젝트 삭제
        Destroy(enemy.gameObject);
    }

}
