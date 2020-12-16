using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

    public void Click_LevelOne()
    {
        SceneManager.LoadScene(1);
    }
    public void Click_LevelTwo()
    {
        SceneManager.LoadScene(2);
    }
    public void Click_Credit()
    {
        SceneManager.LoadScene(3);
    }


}