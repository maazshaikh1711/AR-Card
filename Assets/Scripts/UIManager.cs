using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject welcomeUI;
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject menuUI;
    public GameObject createUI;
    public GameObject updateUI;
    public GameObject deleteUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void ClearScreen()
    {
        welcomeUI.SetActive(false);
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        menuUI.SetActive(false);
        createUI.SetActive(false);
        updateUI.SetActive(false);
        deleteUI.SetActive(false);
    }

    //Functions to change the screens in UI
    public void WelcomeScreen()
    {
        ClearScreen();
        welcomeUI.SetActive(true);
    }
    
    public void LoginScreen()
    {
        ClearScreen();
        loginUI.SetActive(true);
    }   
    public void RegisterScreen()
    {
        ClearScreen();
        registerUI.SetActive(true);
    }

    public void MenuScreen()
    {
        ClearScreen();
        menuUI.SetActive(true);
    }

    public void CreateScreen()
    {
        ClearScreen();
        createUI.SetActive(true);
    }
    
    public void ScanScreen()
    {
        ClearScreen();
        SceneManager.LoadScene("VuforiaScene");
        SceneManager.UnloadSceneAsync("SampleScene");
    }

    public void UpdateScreen()
    {
        ClearScreen();
        updateUI.SetActive(true);
    }

    public void DeleteScreen()
    {
        ClearScreen();
        deleteUI.SetActive(true);
    }

}
