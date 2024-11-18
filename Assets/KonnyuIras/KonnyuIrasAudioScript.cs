using UnityEngine;

public class KonnyuIrasAudioScript : MonoBehaviour
{
    public AudioSource upgradeSuccessAudio;
    public AudioSource successAudio;
    public AudioSource failedAudio;
    public float timeThreshold = 15f; // Variable for setting the threshold in Unity

    // Start is called before the first frame update
    void Start()
    {
        if (KonnyuIrasScript.missedCounter >= 3)
        {
            failedAudio.Play();
        }
        else if (KonnyuIrasScript.currentTime <= timeThreshold && KonnyuIrasScript.missedCounter == 0)
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