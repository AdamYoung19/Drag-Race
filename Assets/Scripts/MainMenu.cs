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

    public string mainMenuSceneName = "Title Screen"; //Set the name of the main menu scene
    public void MainScreen() //This function is going to load the main menu when button is pressed
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public string mainOptionsMenu = "OptionsMenu"; //Set the name of the Options scene

    public void OptionsMenu()
    {
        SceneManager.LoadScene(mainOptionsMenu); //Loads Options Scene
    }
}
