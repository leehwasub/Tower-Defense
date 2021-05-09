using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDetector : MonoBehaviour
{
    [SerializeField]
    private TowerSpawner TowerSpawner;

    private Camera mainCamera;
    private Ray ray;
    private RaycastHit hit;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        //마우스 왼쪽버튼을 눌렀을때
        if (Input.GetMouseButton(0))
        {
            //카레마 위치에서 화면의 마우스 위치를 관총하는 광선생성
            // ray.origin : 광선의 시작위치(=카메라 위치)
            // ray.direction : 광선의 진행방향
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                //2d 모니터를 통해 3d월드의 오브젝트를 마우스로 선택하는 방법
                if (hit.transform.CompareTag("Tile"))
                {
                    //타워를 생성하는 SpawnTower() 호출
                    TowerSpawner.SpawnTower(hit.transform);
                }
            }
        }
    }
}
