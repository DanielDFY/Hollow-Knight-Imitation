using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public void clickStartButton()
    {
        PlayerPrefs.SetString("Milestone", "Spawn");
        clickLoadButton();
    }

    public void clickLoadButton()
    {
        SceneManager.LoadScene(PlayerPrefs.GetString("Milestone"));
    }

    public void clickQuitButton()
    {
        Application.Quit();
    }
}
