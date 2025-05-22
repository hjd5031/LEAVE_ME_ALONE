using Unity.Behavior;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour
{
    public GameObject sprayer; // 손에 붙은 분무통 오브젝트
    public GameObject basket;  // 손에 붙은 바구니 오브젝트
    public GameObject instantiateCar;
    // public BehaviorGraph graph;
    public bool isWatering = false;
    public bool isPicked = false;
    public bool hasItemCar = false;
    void Update()
    {
        StickToGround();
        if (hasItemCar)
        {
            CheckItemAvailability();
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

    void CheckItemAvailability()
    {
        GameObject []PlayerTomato = GameObject.FindGameObjectsWithTag("RipePlayerTomato");
        {
            if (PlayerTomato.Length > 3)
            {
                Instantiate(instantiateCar, transform.position, Quaternion.identity);
                hasItemCar = false;
            }
        } 
    }
}