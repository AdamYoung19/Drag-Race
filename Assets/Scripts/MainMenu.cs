using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame() //This function loads the next scene after button is pressed
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
    }

    //public void MainScreen() //This function is going to load the main menu when button is pressed
    //{
    //    ///SceneManager.LoadScene(SceneManager.SetActiveScene(0));
    //}

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OptionsMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 3);
    }

}
