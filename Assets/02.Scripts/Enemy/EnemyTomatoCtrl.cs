using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyTomatoCtrl : MonoBehaviour
{
    public List<GameObject> tomatoList;//성장 중 prefab 변화를 위한 list

    public GameObject check;//익음 표시 오브젝트
    public GameObject sun;//익는 중 표시 오브젝트
    
    
    
    //토마토의 상태 변화를 위한 불 변수
    public bool isWatering = false;
    public bool isPicked = false;
    public bool isSeeding = false;
    
    public bool enemyUsing;
    public bool playerUsing;
    
    
    private GameObject _currentTomato;//EnemyTomatoPrefab 하위에 있는 진짜 토마토(자식 오브젝트)
    
    private int _growthLevel;//현재 토마토의 성장 단계
    
    private float _growTimer = 0f;
    private float _growDelay = 2f; // 물을 줄 때마다 2초마다 성장
    private float _ripeningTime = 10f;//다 자라고 익은데까지의 시간

    private bool _isGettingSun = false;
    private bool _isRipen = false;
    void Start()
    {
        gameObject.tag = "EnemyisPlantable";//처음에는 빈땅 상태 아무것도 심지 않음
    }

    void Update()
    {
        StatusIconCtrlForTomato();
        CheckTomatoInitializeCondition();
        TomatoGrowProcess();
    }
    
    
    //-------------------------------------------------------------------------------------------------------Tomato Growth bound
    void TomatoGrowProcess()
    {
        if (!isPicked)//플레이어가 수확하고 있지 않은 상태
        {
            if (!isWatering) return;

            _growTimer += Time.deltaTime;

            if (_growTimer >= _growDelay)
            {
                Grow();
                _growTimer = 0f;
            }

            if (_growthLevel == 4)
            {
                _isGettingSun = true;
                gameObject.tag = "EnemyisSunning";
                Invoke("TomatoRipenSun", _ripeningTime);
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
            if (enemyUsing&& _growthLevel ==5 && isPicked)
            {
                int itemRandint = Random.Range(0, 3);
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
            _isGettingSun = false;
            _isRipen = false;
            isPicked = false;
            gameObject.tag = "PickedEnemyTomato";
        }
    }
    
    void StatusIconCtrlForTomato()
    {
        if (sun != null) sun.SetActive(_isGettingSun);
        if (check != null) check.SetActive(_isRipen);
    }

    void CheckTomatoInitializeCondition()
    {
        if (isSeeding && (CompareTag("EnemyisPlantable") || CompareTag("PickedEnemyTomato")))
        {
            InitializeTomatoStatus();
        }
    }

    void InitializeTomatoStatus()//Change Tomato Prefab into Seed(Lev 0)
    {
        _growthLevel = 0;
        DestroyTomato();
        gameObject.tag = "UnripeEnemyTomato";
        isPicked = false;
        isWatering = false;
        isSeeding = false;
        _isGettingSun = false;
        _isRipen = false;
        Invoke("CallInitailizeTomatoPrefab",4f);
    }

    void CallInitailizeTomatoPrefab()//Change Tomato Prefab into Seed(Lev 0)
    {
        SpawnNextTomato(0);
    }
    void Grow()
    {
        if (_growthLevel + 1 >= 5)
        {
            Invoke("TomatoRipenSun", _ripeningTime);
            return;
        }
        _growthLevel++;
        SpawnNextTomato(_growthLevel);
    }

    void SpawnNextTomato(int level)
    {
        DestroyTomato();
        _currentTomato = Instantiate(tomatoList[level], transform.position, Quaternion.identity, transform);
       ChangeColliderHeightbyLevel(level);
    }

    void ChangeColliderHeightbyLevel(int level)
    {
        float height = 1.5f;
        switch (level)//단계별 Collider 높이
        {
            case 0: height = 1.5f; break;
            case 1: 
            case 2: height = 2.1f; break;
            case 3: height = 2.6f; break;
            case 4: 
            case 5: height = 4.73f; break;
        }
        
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)//Collider에 적용
        {
            col.height = height;
        }
    }

    void TomatoRipenSun()
    {
        _isGettingSun = false;
        _isRipen = true;
        gameObject.tag = "RipeEnemyTomato";
    }
    //-------------------------------------------------------------------------------------------------------Tomato Growth bound
    //-------------------------------------------------------------------------------------------------------Item bound
    public void GrowDelayDecrease()//Interact with boost left-time slider
    { 
        _growDelay = 1f;
        _ripeningTime = 5f;
        Invoke("GrowDelayIncrease", 60f);
    }
    void GrowDelayIncrease()//recover grow delay and ripening time
    {
        _growDelay = 2f;
        _ripeningTime = 10f;
    }

    public void ToxicOnTomato()
    {
        _isGettingSun = false;
        _isRipen = false;
        isPicked = false;
        _growDelay = 2f;
        _growthLevel = 0;
        _ripeningTime = 10f;
        CancelInvoke(nameof(TomatoRipenSun));
        DestroyTomato();
        gameObject.tag = "EnemyisPlantable";
    }
    
    void OnTriggerEnter(Collider other)//Collision with Vehicle
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
                _isGettingSun = false;
                _isRipen = false;
                isPicked = false;
                _growthLevel = 0;
                CancelInvoke(nameof(TomatoRipenSun));
                DestroyTomato();
                gameObject.tag = "EnemyisPlantable";
            }
        }
    }
    //-------------------------------------------------------------------------------------------------------Item bound
    void DestroyTomato()
    {
        if (_currentTomato != null)
        {
            Destroy(_currentTomato);
        }
    }
}
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireSphere(transform.position, 0.3f);
    // }
