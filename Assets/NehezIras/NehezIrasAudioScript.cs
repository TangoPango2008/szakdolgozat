using UnityEngine;

public class NehezIrasAudioScript : MonoBehaviour
{
    public AudioSource upgradeSuccessAudio;
    public AudioSource successAudio;
    public AudioSource failedAudio;
    public float timeThreshold = 15f; // Variable for setting the threshold in Unity

    // Start is called before the first frame update
    void Start()
    {
        if (NehezIrasScript.missedCounter >= 3)
        {
            failedAudio.Play();
        }
        else if (NehezIrasScript.currentTime <= timeThreshold && NehezIrasScript.missedCounter == 0)
        {
            upgradeSuccessAudio.Play();
        }
        else
        {
            successAudio.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}