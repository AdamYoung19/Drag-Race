using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PodiumManager : MonoBehaviour
{
    [Header("Podium Setup")]
    public SpriteRenderer winnerSpriteRenderer; // Assign the SpriteRenderer for the winner
    public SpriteRenderer loserSpriteRenderer; // Assign the SpriteRenderer for the loser

    [Header("UI Elements")]
    public TextMeshProUGUI winnerText; // Assign the TextMeshPro for the winner's name
    void Start()
    {
        // Retrieve winner and loser names and sprite names from PlayerPrefs
        string winnerName = PlayerPrefs.GetString("PodiumWinner", "Winner");
        string playerSpriteName = PlayerPrefs.GetString("PlayerSprite", "");
        string computerSpriteName = PlayerPrefs.GetString("ComputerSprite", "");

        // Display names in the UI
        if (winnerText != null) winnerText.text = $"Winner: {winnerName}";

        // Assign the winner's sprite
        Sprite winnerSprite = FindSpriteByName(playerSpriteName);
        if (winnerSprite != null && winnerSpriteRenderer != null)
        {
            winnerSpriteRenderer.sprite = winnerSprite;
            Debug.Log($"Assigned Winner Sprite: {playerSpriteName}");
        }
        else
        {
            Debug.LogError($"Could not assign Winner Sprite: {playerSpriteName}");
        }

        // Assign the loser's sprite
        Sprite loserSprite = FindSpriteByName(computerSpriteName);
        if (loserSprite != null && loserSpriteRenderer != null)
        {
            loserSpriteRenderer.sprite = loserSprite;
            Debug.Log($"Assigned Loser Sprite: {computerSpriteName}");
        }
        else
        {
            Debug.LogError($"Could not assign Loser Sprite: {computerSpriteName}");
        }
    }

    private Sprite FindSpriteByName(string spriteName)
    {
        // Find all SpriteRenderer objects in the scene
        foreach (SpriteRenderer renderer in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (renderer.sprite != null && renderer.sprite.name == spriteName)
            {
                return renderer.sprite;
            }
        }

        Debug.LogError($"Sprite with name '{spriteName}' not found in active objects!");
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