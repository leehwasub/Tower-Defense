using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectDetector : MonoBehaviour
{
    [SerializeField]
    private TowerSpawner TowerSpawner;
    [SerializeField]
    private TowerDataViewer towerDataViewer;

    private Camera mainCamera;
    private Ray ray;
    private RaycastHit hit;
    private Transform hitTransform = null; //마우스 픽킹으로 선택한 오브젝트 임시 저장

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        //마우스가 UI에 머물러 있을때는 아래 코드가 실행되지 않도록함 (버튼 클릭할때 타워 정보가 비활성화 되지 않도록함)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        //마우스 왼쪽버튼을 눌렀을때
        if (Input.GetMouseButton(0))
        {
            //카레마 위치에서 화면의 마우스 위치를 관총하는 광선생성
            // ray.origin : 광선의 시작위치(=카메라 위치)
            // ray.direction : 광선의 진행방향
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                hitTransform = hit.transform;

                //2d 모니터를 통해 3d월드의 오브젝트를 마우스로 선택하는 방법
                if (hit.transform.CompareTag("Tile"))
                {
                    //타워를 생성하는 SpawnTower() 호출
                    TowerSpawner.SpawnTower(hit.transform);
                }
                else if (hit.transform.CompareTag("Tower"))
                {
                    towerDataViewer.OnPanel(hit.transform);
                }
            }
        } else if (Input.GetMouseButtonUp(0))
        {
            //마우스를 눌렀을 때 선택한 오브젝트가 없거나 선택한 오브젝트가 타워가 아니면
            Debug.Log(hitTransform);
            if(hitTransform == null || !hitTransform.CompareTag("Tower"))
            {
                //타워 정보 패턴을 비활성화 한다.
                towerDataViewer.OffPanel();
            }
            hitTransform = null;
        }
    }
}
