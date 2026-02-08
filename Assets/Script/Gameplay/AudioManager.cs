using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Player SFX")] 
    public AudioClip carDamage;
    public AudioClip carExplode;

    [Header("Zombie SFX")] 
    public AudioClip zombieSFX;
    public AudioClip zombieDeath;
    
    [SerializeField] private AudioClip m_music;
    
    
    [SerializeField] private AudioSource m_musicAudio;
    [SerializeField] private AudioSource m_sfxAudio;

    public void Awake()
    {
        Instance = this;
    }

    public void PlaySFX(AudioClip clip)
    {
        m_sfxAudio.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip)
    {
        m_musicAudio.Stop();
        m_musicAudio.clip = clip;
        m_musicAudio.Play();
    }
}
