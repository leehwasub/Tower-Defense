using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyDestroyType { Kill = 0, Arrive }

public class Enemy : MonoBehaviour
{
    private int wayPointCount; //이동 경로 개수
    private Transform[] wayPoints; //이동 경로 정보
    private int currentIndex = 0; //현재 목표지점 인덱스
    private Movement2D movement2D; //오브젝트 이동 제어
    private EnemySpawner enemySpawner; //적의 삭제를 본인이 하지 않고 EnemySpawner에 알려서 삭제
    [SerializeField]
    private int gold = 10; //적 사망시 획득가능한 골드

    public void Setup(EnemySpawner enemySpawner, Transform[] wayPoints)
    {
        this.enemySpawner = enemySpawner;
        movement2D = GetComponent<Movement2D>();
        
        //적 이동 경로 wayPoints 정보 설정
        wayPointCount = wayPoints.Length;
        this.wayPoints = new Transform[wayPointCount];
        this.wayPoints = wayPoints;

        //적의 위치를 첫번째 wayPoint 위치로 설정
        transform.position = wayPoints[currentIndex].position;

        //적 이동/목표지점 설정 코루틴 함수 시작
        StartCoroutine("OnMove");
    }

    private IEnumerator OnMove()
    {
        //다음 이동 방향 설정
        NextMoveTo();
        while (true)
        {
            //적 오브젝트 회전
            transform.Rotate(Vector3.forward * 10);


            if(Vector3.Distance(transform.position, wayPoints[currentIndex].position) < 0.02f * movement2D.MoveSpeed)
            {
                //다음 이동 방향 설정
                NextMoveTo();
            }

            yield return null;
        }
    }

    private void NextMoveTo()
    {
        //아직 이동할 wayPoints가 남아있다면
        if (currentIndex < wayPointCount - 1)
        {
            //적의 위치를 정확하게 목표 위치로 설정
            transform.position = wayPoints[currentIndex].position;

            //이동 방향 설정 => 다음 목표지점 (wayPoints)
            currentIndex++;
            Vector3 direction = (wayPoints[currentIndex].position - transform.position).normalized;
            movement2D.MoveTo(direction);
        }
        //현재 위치가 마지막 wayPoints이면
        else
        {
            gold = 0;
            //적 오브젝트 삭제
            OnDie(EnemyDestroyType.Arrive);
        }
    }

    public void OnDie(EnemyDestroyType type)
    {
        // EnemySpawner에서 리스트로 적 정보를 관리하기 때문에 Destroy()를 직접하지않고
        // EnemySpawner에서 본인이 삭제될 때 필요한 처리를 하도록 DestoryEnemy() 함수 호출
        enemySpawner.DestroyEnemy(type, this, gold);
    }

}
