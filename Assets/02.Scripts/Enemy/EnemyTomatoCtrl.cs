using System.Collections.Generic;
using UnityEngine;

public class EnemyTomatoCtrl : MonoBehaviour
{
    public List<GameObject> tomatoList;

    private int _growthLevel;
    
    private GameObject _currentTomato;
    public GameObject Check;
    public GameObject Sun;
    
    public bool isWatering = false;
    public bool isPicked = false;
    public bool isSeeding = false;
    private float growTimer = 0f;
    private float growDelay = 2f; // 물을 줄 때마다 2초마다 성장 //gamemanager
    private float ripeningTime = 10f;
    public bool EnemyUsing;
    public bool PlayerUsing;

    [SerializeField]private bool isGettingSun = false;
    [SerializeField]private bool isRipen = false;
    void Start()
    {
        // SpawnNextTomato(5);
        gameObject.tag = "EnemyisPlantable";//처음에는 빈땅 상태 아무것도 심지 않음
    }

    void Update()
    {
        if (Sun != null) Sun.SetActive(isGettingSun);
        if (Check != null) Check.SetActive(isRipen);
        if (isSeeding && (CompareTag("EnemyisPlantable") || CompareTag("PickedEnemyTomato")))
        {
            InitializeTomatoStatus();
        }
        if (!isPicked)//플레이어가 수확하고 있지 않은 상태
        {
            if (!isWatering) return;

            growTimer += Time.deltaTime;

            if (growTimer >= growDelay)
            {
                Grow();
                growTimer = 0f;
            }

            if (_growthLevel == 4)
            {
                isGettingSun = true;
                gameObject.tag = "EnemyisSunning";
                Invoke("TomatoRipenSun", ripeningTime);
            }

        }
        else//플레이어가 수확하고 열매 없는 프리팹으로 전환
        {
            if (_currentTomato != null)
            {
                Destroy(_currentTomato);
            }
            _growthLevel = 5;
            SpawnNextTomato(5);
            if (EnemyUsing&& _growthLevel ==5 && isPicked)
            {
                int itemRandint = Random.Range(0, 3);
                Debug.Log("Enemy random itemint" + itemRandint);
                // Debug.Log("itemgiven");
                GameObject enemy = GameObject.FindWithTag("Enemy");
                var enemyScript = enemy.GetComponent<EnemyCtrl>();
                if (itemRandint == 0 && !enemyScript.hasVehicleItem)
                {
                    enemyScript.hasVehicleItem = true;
                }
            
                if (itemRandint == 2 && !enemyScript.hasDroneItem)
                {
                    enemyScript.hasDroneItem = true;
                }
            }
            // if (EnemyUsing)
            // {
            //     GameObject player = GameObject.FindGameObjectWithTag("Enemy");
            //     var playerScript = player.GetComponent<EnemyCtrl>();
            //     // Debug.Log(player);
            //     // int randint = Random.Range(1, 11);
            //     int randint = 1;
            //     if (randint == 1) playerScript.hasVehicleItem = true;
            // }
            // SpawnNextTomato(5);
            isGettingSun = false;
            isRipen = false;
            isPicked = false;
            gameObject.tag = "PickedEnemyTomato";
        }
        
        
    }

    void InitializeTomatoStatus()
    {
        _growthLevel = 0;
        if (_currentTomato != null)
        {
            Destroy(_currentTomato);
        }
        gameObject.tag = "UnripeEnemyTomato";
        isPicked = false;
        isWatering = false;
        isSeeding = false;
        isGettingSun = false;
        isRipen = false;
        // SpawnNextTomato(0);

        // Debug.Log("TomatoStatus Initialized");
        Invoke("CallInitailizeTomatoPrefab",4f);
    }

    void CallInitailizeTomatoPrefab()
    {
        SpawnNextTomato(0);
    }
    void Grow()
    {
        if (_growthLevel + 1 >= 5)
        {
            Invoke("TomatoRipenSun", ripeningTime);
            return;
        }
        _growthLevel++;
        SpawnNextTomato(_growthLevel);
        // gameObject.tag = "UnripeEnemyTomato"; // 성장 중간도 태그 유지
    }

    void SpawnNextTomato(int level)
    {
        if (_currentTomato != null)
        {
            Destroy(_currentTomato);
        }

        // Debug.Log("TomatoLevel: " + _growthLevel);
        _currentTomato = Instantiate(tomatoList[level], transform.position, Quaternion.identity, transform);

        // 🌱 콜라이더 높이 설정
        float height = 1.5f;
        switch (level)
        {
            case 0: height = 1.5f; break;
            case 1: 
            case 2: height = 2.1f; break;
            case 3: height = 2.6f; break;
            case 4: 
            case 5: height = 4.73f; break;
        }

        // 🟢 CapsuleCollider 기준 예시
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            col.height = height;
            // col.center = new Vector3(0, height / 2f, 0); // 중심 높이 재조정
        }
        // int itemRandint = Random.Range(0, 2);
        // if (EnemyUsing&& _growthLevel ==5)
        // {
        //     // Debug.Log("itemgiven");
        //     GameObject enemy = GameObject.FindWithTag("Enemy");
        //     var enemyCtrl = enemy.GetComponent<EnemyCtrl>();
        //     if (itemRandint == 0 && !enemyCtrl.hasVehicleItem)
        //     {
        //         int randint = Random.Range(0, 2);
        //         if (randint == 1) enemyCtrl.hasVehicleItem = true;
        //     }
        //
        //     if (itemRandint == 1 && !enemyCtrl.hasDroneItem)
        //     {
        //         int randint = Random.Range(0, 2);
        //         if (randint == 1) enemyCtrl.hasDroneItem = true;
        //     }
        // }
    }

    void TomatoRipenSun()
    {
        isGettingSun = false;
        isRipen = true;
        gameObject.tag = "RipeEnemyTomato";
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }

    public void GrowDelayDecrease()
    { 
        Debug.Log("GrowDelayDecrease");
        growDelay = 1f;
        ripeningTime = 5f;
        //todo activate boost icon
        Invoke("GrowDelayIncrease", 60f);
    }
    void GrowDelayIncrease()
    {
        growDelay = 2f;
        ripeningTime = 10f;
    }

    public void ToxicOnTomato()
    {
        isGettingSun = false;
        isRipen = false;
        //todo isGettingBosst = false;
        isPicked = false;
        growDelay = 2f;
        _growthLevel = 0;
        ripeningTime = 10f;
        CancelInvoke(nameof(TomatoRipenSun));
        if(_currentTomato!= null)
            Destroy(_currentTomato);
        gameObject.tag = "EnemyisPlantable";
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCar"))
        {
            if (gameObject.tag == "UnripeEnemyTomato")
            {
                SpawnNextTomato(0);
                _growthLevel = 0;
            }
            else
            {
                isGettingSun = false;
                isRipen = false;
                isPicked = false;
                _growthLevel = 0;
                CancelInvoke(nameof(TomatoRipenSun));
                Destroy(_currentTomato);
                gameObject.tag = "EnemyisPlantable";
            }
        }
    }
}