using UnityEngine;
using System.Collections.Generic; 
using TMPro;
using UnityEngine.SceneManagement;

public class PodiumManager : MonoBehaviour
{
    [Header("Podium Setup")]
    public SpriteRenderer winnerSpriteRenderer; // Assign the SpriteRenderer for the winner
    public SpriteRenderer loserSpriteRenderer; // Assign the SpriteRenderer for the loser

    [Header("UI Elements")]
    public TextMeshProUGUI winnerText; // Assign the TextMeshPro for the winner's name
    
    [Header("Car Data")]
    public List<CarData> availableCars; // Assign all CarData assets in the Inspector

    void Start()
    {
        
        string PodiumWinner = PlayerPrefs.GetString("PodiumWinner", "");
        string PodiumLoser = PlayerPrefs.GetString("PodiumLoser", "");

        string WinnerCarName = "";
        string LoserCarName = "";

        if (PodiumWinner == "Player")
        {
            WinnerCarName = PlayerPrefs.GetString("SelectedPlayerCarName", "");
            winnerText.text = "Player Wins!"; // Set the winner text
        }
        else if (PodiumWinner == "Computer")
        {
            WinnerCarName = PlayerPrefs.GetString("SelectedOpponentCarName", "");
            winnerText.text = "Computer Wins!"; // Set the winner text
        }

        if (PodiumLoser == "Player")
        {
            LoserCarName = PlayerPrefs.GetString("SelectedPlayerCarName", "");
        }
        else if (PodiumLoser == "Computer")
        {
            LoserCarName = PlayerPrefs.GetString("SelectedOpponentCarName", "");
        }
        
        if (!string.IsNullOrEmpty(WinnerCarName))
        {
            // Find the CarData object by name
            CarData WinnerData = FindCarDataByName(WinnerCarName);

            if (WinnerData != null && winnerSpriteRenderer != null)
            {
                // Assign the sprite to the SpriteRenderer
                winnerSpriteRenderer.sprite = WinnerData.carSprite;
                Debug.Log($"Assigned sprite for car: {WinnerData.carName}");
            }
            else
            {
                Debug.LogError($"CarData for '{WinnerCarName}' not found or sprite is missing.");
            }
        }
        else
        {
            Debug.LogError("No car name found in WinnerPrefs.");
        }
        
        if (!string.IsNullOrEmpty(LoserCarName))
        {
            // Find the CarData object by name
            CarData LoserData = FindCarDataByName(LoserCarName);

            if (LoserData != null && loserSpriteRenderer != null) // Corrected variable name
            {
                // Assign the sprite to the SpriteRenderer
                loserSpriteRenderer.sprite = LoserData.carSprite; // Corrected variable name
                Debug.Log($"Assigned sprite for car: {LoserData.carName}");
            }
            else
            {
                Debug.LogError($"CarData for '{LoserCarName}' not found or sprite is missing.");
            }
        }
        else
        {
            Debug.LogError("No car name found in LoserPrefs.");
        }
    }
    private CarData FindCarDataByName(string carName)
    {
        foreach (CarData carData in availableCars) // Ensure availableCars is assigned in PodiumManager
        {
            if (carData.name == carName)
            {
                return carData;
            }
        }
        Debug.LogError($"CarData not found for car name: {carName}");
        return null;
    }
    public void ReturnToSelection()
    {
        SceneManager.LoadScene(1);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}