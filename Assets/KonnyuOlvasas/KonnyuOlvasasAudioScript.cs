using UnityEngine;

public class KonnyuOlvasasAudioScript : MonoBehaviour
{
    public AudioSource greaterAudio;
    public AudioSource smallerAudio;
    public float timeThreshold = 15f; // Variable for setting the threshold in Unity

    // Start is called before the first frame update
    void Start()
    {
        if (KonnyuOlvasasScript.CurrentTime >= timeThreshold)
        {
            greaterAudio.Play();
        }
        else
        {
            smallerAudio.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}