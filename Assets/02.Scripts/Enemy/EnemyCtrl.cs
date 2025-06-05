using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyCtrl : MonoBehaviour
{
    public GameObject sprayer; // 손에 붙은 분무통 오브젝트
    public GameObject basket;  // 손에 붙은 바구니 오브젝트
    public GameObject instantiateCar;
    public GameObject toxicDroneItem;//for player
    public GameObject boostDroneItem;//for Enemy
    public Transform toxicDroneSpawnPoint;//player side
    public Transform boostDroneSpawnPoint;//enemy side
    
    
    // public BehaviorGraph graph;
    public bool isWatering = false;
    public bool isPicked = false;
    public bool hasVehicleItem = false;
    public bool hasDroneItem = false;
    public int enemyScore = 0; //GameManager
    
    
    void Update()
    {
        EnemyScoreUpdate();
        StickToGround();
        EnemyCheckItemCondition(); 
        EnemyObjectControl();
    }

    
    
    void EnemyScoreUpdate()
    {
        GameManager.Instance.EnemyScore = enemyScore;
    }

    void EnemyObjectControl()//애니메이터 전이에 따른 적 손 아이템 변경
    {
        // 분무 상태
        if (isWatering)
        {
            if (!sprayer.activeSelf) sprayer.SetActive(true);
            if (basket.activeSelf) basket.SetActive(false);
        }
        // 수확 상태
        else if (isPicked)
        {
            if (!basket.activeSelf) basket.SetActive(true);
            if (sprayer.activeSelf) sprayer.SetActive(false);
        }
        // 아무것도 안할 때
        else
        {
            if (sprayer.activeSelf) sprayer.SetActive(false);
            if (basket.activeSelf) basket.SetActive(false);
        }
    }
    
    void StickToGround()//적이 바닥에 떠 있는것을 방지 raycast로 거리 측정
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float rayLength = 2f;

        if (Physics.Raycast(origin, Vector3.down, out hit, rayLength))
        {
            Vector3 pos = transform.position;
            // 서서히 보정 (즉시 고정하지 않고 Lerp로 부드럽게)
            pos.y = Mathf.Lerp(pos.y, hit.point.y, 0.2f);
            transform.position = pos;
        }
    }

    void EnemyCheckItemCondition()
    {
        if (hasVehicleItem)
        {
            CheckVehicleItemAvailability();
        }

        if (hasDroneItem)
        {
            CheckDroneItemAvailability();
        }
    }
    void CheckVehicleItemAvailability()//적의 자동차 아이템 사용 판단을 위한 함수입니다. 플레이어의 토마토 농장 상황에 따라 사용을 판단합니다.
    {
        GameObject []ripePlayerTomato = GameObject.FindGameObjectsWithTag("RipePlayerTomato");
        GameObject []unripePlayerTomato = GameObject.FindGameObjectsWithTag("UnripePlayerTomato");
        GameObject []pickedPlayerTomato = GameObject.FindGameObjectsWithTag("PickedPlayerTomato");
        int randInt = Random.Range(0, 2);
        if (!GameManager.Instance.PLayerUsingItem&& (ripePlayerTomato.Length + unripePlayerTomato.Length + pickedPlayerTomato.Length) >= 3&& randInt == 0)
        {
            Instantiate(instantiateCar, transform.position, Quaternion.identity);
            GameManager.Instance.EnemyUsingItem = true;
            hasVehicleItem = false;
        }
        else if (hasDroneItem&&!GameManager.Instance.PLayerUsingItem &&
                 (ripePlayerTomato.Length + unripePlayerTomato.Length + pickedPlayerTomato.Length) >= 3 && randInt == 1)
        {
            Instantiate(toxicDroneItem, toxicDroneSpawnPoint.position, Quaternion.identity);
            hasDroneItem = false;
        }
        
    }

    void CheckDroneItemAvailability()//적의 부스트 드론 아이템 사용 판단 함수. 얻자마자 바로 사용
    {
        int randint = Random.Range(0, 3);
        if (randint == 1)
        {
            Instantiate(boostDroneItem, boostDroneSpawnPoint.position, Quaternion.identity);
            hasDroneItem = false;
        } 
    }
}