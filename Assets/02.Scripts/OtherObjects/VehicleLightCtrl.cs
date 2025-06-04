using System;
using UnityEngine;
using System.Collections;

public class VehicleLightCtrl : MonoBehaviour
{
    public Material material;
    public GameObject[] lights;
    public GameObject mudParticle;
    public Color emissionColor = Color.white; // 기본 발광 색상
    public int flickerCount = 6;              // 총 깜빡임 횟수 (짝수면 false로 끝나므로 5나 7 추천)
    public GameObject introCamera;
    public GameObject followCamera;
    private GameObject crossHair;
    
    
    private String CarEngineSoundID;
    private String CarAccelerationSoundID;
    private String CarHornID;
    // public AudioSource CarHorn;

    void Awake()
    {
    }
    void Start()
    {
        SetEmission(false);
        crossHair = GameObject.FindWithTag("crossHair");
        if(crossHair != null)
            crossHair.SetActive(false);
        CarEngineSoundID = SoundManager.Instance.Play3DSfx(SoundManager.Sfx.EngineStart,transform,1f);
        introCamera.SetActive(true);
        followCamera.SetActive(false);
        mudParticle.SetActive(false);
        StartCoroutine(FlickerSequence());
        // Invoke(nameof(TurnOffCameras),15f);
    }

    IEnumerator FlickerSequence()
    {
        yield return new WaitForSeconds(1f); // ⏳ 3초 대기
        CarHornID = SoundManager.Instance.Play3DSfx(SoundManager.Sfx.CarHorn,transform,1f);
        for (int i = 0; i < flickerCount; i++)
        {
            bool on = i % 2 == 0;

            SetLightsActive(on);
            SetEmission(on);
            // if (on) SoundManager.Instance.Play3DSfx(SoundManager.Sfx.CarHorn,transform,1f);
            yield return new WaitForSeconds(0.4f);
        }
        CarAccelerationSoundID = SoundManager.Instance.Play3DSfx(SoundManager.Sfx.CarAcceleration,transform,1f);
        // SoundManager.Instance.Play3DSfx(SoundManager.Sfx.CarHorn,transform,1f);
        // 마지막은 항상 true로 고정
        
        introCamera.SetActive(false);
        followCamera.SetActive(true);
        mudParticle.SetActive(true);
        SetLightsActive(true);
        SetEmission(true);
        
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.StopSfx(CarHornID);
    }

    void TurnOffCameras()
    {
        introCamera.SetActive(false);
        followCamera.SetActive(false);
        SoundManager.Instance.StopSfx(CarAccelerationSoundID);
        if(crossHair != null && crossHair.activeSelf == false)
            crossHair.SetActive(true);
        if (GameManager.Instance.PLayerUsingItem)
        {
            GameManager.Instance.PLayerUsingItem = false;
            return;
        }

        GameManager.Instance.EnemyUsingItem = false;
    }
    void SetLightsActive(bool isOn)
    {
        foreach (GameObject obj in lights)
        {
            if (obj != null)
                obj.SetActive(isOn);
        }
    }

    void SetEmission(bool isOn)
    {
        if (material != null)
        {
            if (isOn)
            {
                material.EnableKeyword("_EMISSION");
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnterVehicleLightCtrl");
        Invoke(nameof(TurnOffCameras),2f);
    }
}