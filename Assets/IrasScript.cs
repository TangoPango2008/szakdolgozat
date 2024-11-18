using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IrasScript : MonoBehaviour
{
    public Button skipButton;
    public AudioSource instructionAudio;
    public AudioSource irasAudio;
    public GameObject instructionPanel;


    // Start is called before the first frame update
    void Start()
    {
        irasAudio.Stop();
        Cursor.lockState = CursorLockMode.None;
        skipButton.onClick.AddListener(() =>
        {
            instructionPanel.SetActive(false);
            if (instructionAudio.isPlaying) // Check if the audio is playing
            {
                instructionAudio.Stop(); // Stop the audio
            }
            irasAudio.Play();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BackClick()
    {
        StaticData.Iras = false;
        SceneManager.LoadScene(0);
    }

    public void KonnyuClick()
    {
        SceneManager.LoadScene(7);
    }

    public void KozepesClick()
    {
        SceneManager.LoadScene(8);
    }

    public void NehezClick()
    {
        SceneManager.LoadScene(9);
    }
}
