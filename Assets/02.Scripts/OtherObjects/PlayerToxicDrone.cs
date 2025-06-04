using System.Collections;
using UnityEngine;

public class PlayerToxicDrone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ParticleSystem LiquidParticle;
    private string DroneFlySoundID;

    private string DroneSpraySoundID;
    void Start()
    {
        LiquidParticle.Stop();
        DroneFlySoundID = SoundManager.Instance.Play3DSfx(SoundManager.Sfx.DroneFlying,transform,1f);

        StartCoroutine(DestroyTomatoCoroutine());
    }

    void DestroyTomato()
    {
        foreach(var tomato in GameManager.Instance.playerTomatoes)
        {
            tomato.ToxicOnTomato();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }IEnumerator DestroyTomatoCoroutine()
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