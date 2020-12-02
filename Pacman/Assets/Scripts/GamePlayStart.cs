using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlayStart : MonoBehaviour
{
    // Start is called before the first frame update


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

    public void Click_Start()
    {
        SceneManager.LoadScene(2);
    }
    public void Click_Credit()
    {
        SceneManager.LoadScene(1);
    }


}