using System.Collections;
using UnityEngine;

public class EnemyToxicDrone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ParticleSystem LiquidParticle;
    private string DroneFlySoundID;
    private string DroneSpraySoundID;
    void Start()
    {
        LiquidParticle.Stop();
        DroneFlySoundID = SoundManager.Instance.Play3DSfx(SoundManager.Sfx.DroneFlying,transform,1f);

        StartCoroutine(BoostTomatoCoroutine());
    }

    void DestroyTomato()
    {
        foreach(var tomato in GameManager.Instance.enemyTomatoes)
        {
            tomato.ToxicOnTomato();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }IEnumerator BoostTomatoCoroutine()
    {
        yield return new WaitForSeconds(1);
        LiquidParticle.Play();
        DroneSpraySoundID = SoundManager.Instance.Play3DSfx(SoundManager.Sfx.DroneSpraySound,transform,1f);
        yield return new WaitForSeconds(1);
        DestroyTomato();
        yield return new WaitForSeconds(3);
        
        // SoundManager.Instance.StopSfx(DroneFlySoundID);
        SoundManager.Instance.StopSfx(DroneSpraySoundID);
        LiquidParticle.Stop();
    }
}