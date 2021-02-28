using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls SoundFX
/// </summary>

public class AudioController : MonoBehaviour
{
    static public AudioController S; // Main AudioController attached to the camera
    // Start is called before the first frame update
    [Header("Set In Inspector")]
    // Audio Clips
    public AudioSource audioSource;
    public AudioClip blasterShootClip;
    public AudioClip laserShootClip;
    public AudioClip missileShootClip;
    public AudioClip enemyDamageClip;
    public AudioClip heroDamageClip;
    public AudioClip explosionClip;
    public AudioClip powerUpClip;

    public float volumeLevel = 0.3f;

    void Start()

    {
        // Singleton
        if (S == null)
        {
            S = this;           
        }
        else
        {
            Debug.Log("AudioController.Start() - attempted to assign second AudioController.S!");
        }

        audioSource = this.gameObject.GetComponent<AudioSource>(); // Find Audio Source
    }

    // Play Shooting Blaster SoundFX
    public void PlayBlasterShoot()
    {
        if(audioSource)
        {
            if(blasterShootClip)
            {
                audioSource.PlayOneShot(blasterShootClip, volumeLevel);
            }
        }
    }

    // Play Shooting Laser SoundFX
    public void PlayLaserShoot()
    {
        if (audioSource)
        {
            if (laserShootClip)
            {
                audioSource.PlayOneShot(laserShootClip, volumeLevel/4.0f);
            }
        }
    }

    // Play Flying Missile SoundFX
    public void PlayMissileShoot()
    {
        if (audioSource)
        {
            if (missileShootClip)
            {
                audioSource.PlayOneShot(missileShootClip, volumeLevel/8.0f);
            }
        }
    }

    // Play Enemy Damage SoundFX
    public void PlayEnemyDamage()
    {
        if (audioSource)
        {
            if (enemyDamageClip)
            {
                audioSource.PlayOneShot(enemyDamageClip, volumeLevel*1.5f);
            }
        }
    }

    // Play Hero Damage SoundFX
    public void PlayHeroDamage()
    {
        if (audioSource)
        {
            if (heroDamageClip)
            {
                audioSource.PlayOneShot(heroDamageClip, volumeLevel*1.5f);
            }
        }
    }

    // Play Explosion SoundFX
    public void PlayExplosion()
    {
        if (audioSource)
        {
            if (explosionClip)
            {
                audioSource.PlayOneShot(explosionClip, volumeLevel*1.5f);
            }
        }
    }

    // Play Power Up SoundFX
    public void PlayPowerUp()
    {
        if (audioSource)
        {
            if (powerUpClip)
            {
                audioSource.PlayOneShot(powerUpClip, volumeLevel*2.0f);
            }
        }
    }
}
