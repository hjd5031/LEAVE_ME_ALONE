using System.Collections.Generic;
using UnityEngine;

public class TomatoCtrl : MonoBehaviour
{
    public List<GameObject> tomatoList;

    private int _growthLevel;
    private GameObject _currentTomato;

    public bool isWatering = false;

    private float growTimer = 0f;
    private float growDelay = 3f; // 물을 줄 때마다 3초마다 성장

    void Start()
    {
        SpawnNextTomato(0); // 씨앗만 자동으로 심기
        gameObject.tag = "UnripeEnemyTomato";
    }

    void Update()
    {
        if (!isWatering) return;

        growTimer += Time.deltaTime;

        if (growTimer >= growDelay)
        {
            Grow();
            growTimer = 0f;
        }
    }

    void Grow()
    {
        if (_growthLevel + 1 >= tomatoList.Count)
        {
            gameObject.tag = "RipeEnemyTomato";
            return;
        }

        _growthLevel++;
        SpawnNextTomato(_growthLevel);
        _currentTomato.tag = "UnripeEnemyTomato"; // 성장 중간도 태그 유지
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }

    // // 외부에서 물주기 시작 / 멈춤 제어
    // public void StartWatering()
    // {
    //     isWatering = true;
    // }
    //
    // public void StopWatering()
    // {
    //     isWatering = false;
    // }
}