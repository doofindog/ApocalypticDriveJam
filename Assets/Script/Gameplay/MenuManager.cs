using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public AudioSource m_musicAudio;
    public AudioSource m_sfxAudio;
    
    public AudioClip buttonClickedSFX;
    public GameObject LoadingPannel;
    public float loadTime = 3.0f;
    
    public void StartGame()
    {
        m_sfxAudio.PlayOneShot(buttonClickedSFX);
        StartCoroutine(LoadGame());
    }

    public IEnumerator LoadGame()
    {
        LoadingPannel.SetActive(true);
        
        yield return new WaitForSeconds(loadTime);
        
        SceneManager.LoadScene(1);
    }
}
