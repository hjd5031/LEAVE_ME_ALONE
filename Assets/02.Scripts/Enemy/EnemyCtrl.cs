using Unity.Behavior;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour
{
    public GameObject sprayer; // 손에 붙은 분무통 오브젝트
    public GameObject basket;  // 손에 붙은 바구니 오브젝트
    public GameObject instantiateCar;
    public GameObject ToxicDroneItem;//for player
    public GameObject BoostDroneItem;//for Enemy itself
    public Transform ToxicDroneSpawnPoint;//player side
    public Transform BoostDroneSpawnPoint;//enemy side
    
    
    // public BehaviorGraph graph;
    public bool isWatering = false;
    public bool isPicked = false;
    public bool hasVehicleItem = false;//gamemanager
    public bool hasDroneItem = false;//gamemanager
    public int EnemyScore = 0;//gamemanager
    
    
    // string debugMessage = "";
    void Update()
    {
        GameManager.Instance.EnemyScore = EnemyScore;
        // if (hasVehicleItem && hasDroneItem)
        // {
        //     // ResetCamera();
        //     debugMessage = "has Vehicle Item"+"\nhas Drone Item\n" + EnemyScore;
        // }
        // if (!hasVehicleItem && hasDroneItem)
        // {
        //     // ResetCamera();
        //     debugMessage = "no Vehicle Item"+"\nhas Drone Item\n" + EnemyScore;
        // }
        // if (hasVehicleItem && !hasDroneItem)
        // {
        //     // ResetCamera();
        //     debugMessage = "has Vehicle Item"+"\nno Drone Item\n" + EnemyScore;
        // }
        // if (!hasVehicleItem && !hasDroneItem)
        // {
        //     // ResetCamera();
        //     debugMessage = "no Vehicle Item"+"\nno Drone Item\n" + EnemyScore;
        // }
        
        StickToGround();
        if (hasVehicleItem)
        {
            CheckVehicleItemAvailability();
        }

        if (hasDroneItem)
        {
            CheckDroneItemAvailability();
        }
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
    void StickToGround()
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

    void CheckVehicleItemAvailability()
    {
        GameObject []RipePlayerTomato = GameObject.FindGameObjectsWithTag("RipePlayerTomato");
        GameObject []UnripePlayerTomato = GameObject.FindGameObjectsWithTag("UnripePlayerTomato");
        GameObject []PickedPlayerTomato = GameObject.FindGameObjectsWithTag("PickedPlayerTomato");
        int randint = Random.Range(0, 2);
        if (!GameManager.Instance.PLayerUsingItem&& (RipePlayerTomato.Length + UnripePlayerTomato.Length + PickedPlayerTomato.Length) >= 3&& randint == 0)
        {
            Instantiate(instantiateCar, transform.position, Quaternion.identity);
            GameManager.Instance.EnemyUsingItem = true;
            hasVehicleItem = false;
        }
        else if (!GameManager.Instance.PLayerUsingItem &&
                 (RipePlayerTomato.Length + UnripePlayerTomato.Length + PickedPlayerTomato.Length) >= 3 && randint == 1)
        {
            Instantiate(ToxicDroneItem, ToxicDroneSpawnPoint.position, Quaternion.identity);
            hasDroneItem = false;
        }
        
    }

    void CheckDroneItemAvailability()
    {
        GameObject []RipePlayerTomato = GameObject.FindGameObjectsWithTag("RipePlayerTomato");
        GameObject []UnripePlayerTomato = GameObject.FindGameObjectsWithTag("UnripePlayerTomato");
        int randint = Random.Range(0, 3);
        if (randint == 1)
        {
            Instantiate(BoostDroneItem, BoostDroneSpawnPoint.position, Quaternion.identity);
            hasDroneItem = false;
        } 
        // else if ((RipePlayerTomato.Length + UnripePlayerTomato.Length) >= 3)
        // {
        //     Instantiate(ToxicDroneItem, ToxicDroneSpawnPoint.position, Quaternion.identity);
        //     hasDroneItem = false;
        // }
        
    }
    // void OnGUI()
    // {
    //     GUIStyle style = new GUIStyle(GUI.skin.label);
    //     style.fontSize = 32;
    //     style.normal.textColor = Color.red;
    //     style.alignment = TextAnchor.UpperRight;
    //
    //     // 위치: 오른쪽 상단 (padding 10)
    //     Rect position = new Rect(Screen.width - 410, 10, 400, 120);
    //     GUI.Label(position, debugMessage, style);
    // }
}