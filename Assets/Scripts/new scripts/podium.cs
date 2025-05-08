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

    Debug.Log($"Retrieved Player Sprite Name: {playerSpriteName}");
    Debug.Log($"Retrieved Computer Sprite Name: {computerSpriteName}");

    // Display names in the UI
    if (winnerText != null) winnerText.text = $"Winner: {winnerName}";

    // Assign the winner's sprite
    Sprite winnerSprite = Resources.Load<Sprite>($"Sprites/{playerSpriteName}");
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
    Sprite loserSprite = Resources.Load<Sprite>($"Sprites/{computerSpriteName}");
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

    public void ReturnToSelection()
    {
        SceneManager.LoadScene(1);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}