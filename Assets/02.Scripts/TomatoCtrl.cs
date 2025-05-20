using System.Collections.Generic;
using UnityEngine;

public class TomatoCtrl : MonoBehaviour
{
    public List<GameObject> tomatoList;

    private int _growthLevel;
    private GameObject _currentTomato;

    public bool isWatering = false;
    public bool isPicked = false;
    public bool isSeeding = false;
    private float growTimer = 0f;
    private float growDelay = 2f; // 물을 줄 때마다 3초마다 성장

    void Start()
    {
        gameObject.tag = "EnemyisPlantable";//처음에는 빈땅 상태 아무것도 심지 않음
    }

    void Update()
    {
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
            if (_growthLevel == 4)Invoke("TomatoRipenSun", 5f);

        }
        else//플레이어가 수확하고 열매 없는 프리팹으로 전환
        {
            if (_currentTomato != null)
            {
                Destroy(_currentTomato);
            }

            gameObject.tag = "PickedEnemyTomato";
            SpawnNextTomato(5);
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
        // SpawnNextTomato(0);

        Debug.Log("TomatoStatus Initialized");
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
            Invoke("TomatoRipenSun", 5f);
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
        gameObject.tag = "RipeEnemyTomato";
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}