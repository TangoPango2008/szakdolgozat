using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class NameScript : MonoBehaviour
{
    public InputField InputField;

  
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        InputField.Select();
        InputField.ActivateInputField();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSubmitName()
    {
        StaticData.NameValue = InputField.text;

        if (StaticData.Iras == false)
        {
            SceneManager.LoadScene(2);
        }
        else 
        {
            SceneManager.LoadScene(3);
        }


    }
}
