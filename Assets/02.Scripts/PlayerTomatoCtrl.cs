using System.Collections.Generic;
using UnityEngine;

public class PlayerTomatoCtrl : MonoBehaviour
{
     public List<GameObject> tomatoList;

    private int _growthLevel;
    
    private GameObject _currentTomato;
    public GameObject Check;
    public GameObject Sun;
    
    public bool isWatering = false;
    public bool isPicked = false;
    public bool isSeeding = false;
    // public bool isClicking = false;
    private float growTimer = 0f;
    private float growDelay = 2f; // 물을 줄 때마다 3초마다 성장


    private bool isGettingSun = false;
    private bool isRipen = false;
    void Start()
    {
        gameObject.tag = "PlayerisPlantable";//처음에는 빈땅 상태 아무것도 심지 않음
    }

    void Update()
    {
        if (Sun != null) Sun.SetActive(isGettingSun);
        if (Check != null) Check.SetActive(isRipen);
        if (isSeeding && (CompareTag("PlayerisPlantable") || CompareTag("PickedPlayerTomato")))
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
                gameObject.tag = "PlayerisSunning";
                Invoke("TomatoRipenSun", 10f);
            }

        }
        else//플레이어가 수확하고 열매 없는 프리팹으로 전환
        {
            if (_currentTomato != null)
            {
                Destroy(_currentTomato);
            }
            SpawnNextTomato(5);
            isGettingSun = false;
            isRipen = false;
            gameObject.tag = "PickedPlayerTomato";
        }
    }

    void InitializeTomatoStatus()//플레이어에서 4초 눌러야 isSeeding 전환
    {
        _growthLevel = 0;
        if (_currentTomato != null)
        {
            Destroy(_currentTomato);
        }
        gameObject.tag = "UnripePlayerTomato";
        isPicked = false;
        isWatering = false;
        isSeeding = false;
        isGettingSun = false;
        isRipen = false;
        SpawnNextTomato(0);

        Debug.Log("TomatoStatus Initialized");
        // Invoke("CallInitailizeTomatoPrefab",4f);
    }

    void CallInitailizeTomatoPrefab()
    {
        SpawnNextTomato(0);
    }
    void Grow()
    {
        if (_growthLevel + 1 >= 5)
        {
            Invoke("TomatoRipenSun", 10f);
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

        Debug.Log("TomatoLevel: " + _growthLevel);
        _currentTomato = Instantiate(tomatoList[level], transform.position, Quaternion.identity, transform);
    }

    void TomatoRipenSun()
    {
        isGettingSun = false;
        isRipen = true;
        gameObject.tag = "RipePlayerTomato";
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            
            if (gameObject.tag == "UnripePlayerTomato")
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
                gameObject.tag = "PlayerisPlantable";
            }
        }
    }
}