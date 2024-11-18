using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript: MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        StaticData.NameValue = "";
        StaticData.Iras = false;
    }

    void Update()
    {

    }

    public void OlvasasClick()
    {
        SceneManager.LoadScene(1);
    }

    public void IrasClick()
    {
        StaticData.Iras = true;
        SceneManager.LoadScene(1);
    }

    public void ExitClick()
    {
        Application.Quit();
        Debug.Log("Quit!");
    }
}
