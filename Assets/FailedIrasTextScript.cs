using UnityEngine;
using UnityEngine.UI;

public class StatusMessage : MonoBehaviour
{
    public Text statusText; // Reference to the Text element

    void Update()
    {
        // Check the missedCounter value from the game script
        if (KonnyuIrasScript.missedCounter >= 3)
        {
            statusText.text = "Sajnos nem siker�lt megmentened az �r�s bolyg�j�t!";
        }
        else
        {
            statusText.text = "Megmentetted az �r�s bolyg�j�t!";
        }
    }
}