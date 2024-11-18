using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class OlvasScript : MonoBehaviour
{
    public Button skipButton;
    public AudioSource instructionAudio;
    public AudioSource olvasAudio;
    public GameObject instructionPanel;


    // Start is called before the first frame update
    void Start()
    {
        olvasAudio.Stop();
        Cursor.lockState = CursorLockMode.None;
        skipButton.onClick.AddListener(() =>
        {
            instructionPanel.SetActive(false);
            if (instructionAudio.isPlaying) // Check if the audio is playing
            {
                instructionAudio.Stop(); // Stop the audio
            }
            olvasAudio.Play();
        });
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void BackClick()
    {
        SceneManager.LoadScene(0);
    }

    public void KonnyuClick()
    {
        SceneManager.LoadScene(4);
    }

    public void KozepesClick()
    {
        SceneManager.LoadScene(5);
    }

    public void NehezClick()
    {
        SceneManager.LoadScene(6);
    }

}
