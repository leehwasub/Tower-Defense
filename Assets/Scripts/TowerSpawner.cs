using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject towerPrefab;
    [SerializeField]
    private int towerBuildGold = 50; // 타워 건설에 사용되는 골드
    [SerializeField]
    private EnemySpawner enemySpawner; //현재 맵에 존재하는 적 리스트 정보를 얻기 위해
    [SerializeField]
    private PlayerGold playerGold; //타워 건설시 골드 감소를 위해

    public void SpawnTower(Transform tileTransform)
    {
        if(towerBuildGold > playerGold.CurrentGold)
        {
            return;
        }

        Tile tile = tileTransform.GetComponent<Tile>();

        if (tile.IsBuildTower)
        {
            return;
        }
        //타워가 건설되어있음으로 설정
        tile.IsBuildTower = true;
        //타워 건설에 필요한 골드만큼 감소
        playerGold.CurrentGold -= towerBuildGold;
        // 선택한 타일의 위치에 타워 건설
        GameObject clone = Instantiate(towerPrefab, tileTransform.position, Quaternion.identity);
        // 타워 무기에 enemySpawner 정보 전달
        clone.GetComponent<TowerWeapon>().Setup(enemySpawner);
    }
}
