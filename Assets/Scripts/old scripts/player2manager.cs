using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class player2manager : MonoBehaviour
{
    public CharacterDatabase characterDB;

    public Text p2_nameText;
    public SpriteRenderer p2_artworkSprite;

    private int Player_2 = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!PlayerPrefs.HasKey("Player_2"))
        {
            Player_2 = 0;
        }
        
        else
        {
            Load();
        }
        UpdateCharacter(Player_2);
    }

    public void NextOption()
    {
        Player_2++;
        if (Player_2 >= characterDB.CharacterCount)
        {
            Player_2 = 0;
        }

        UpdateCharacter(Player_2);
        Save();
    }

    public void BackOption()
    {
        Player_2--;
        if (Player_2 < 0)
        {
            Player_2 = characterDB.CharacterCount - 1;
        }

        UpdateCharacter(Player_2);
        Save();
    }

    public void UpdateCharacter(int Player_2)
    {
        Character character = characterDB.GetCharacter(Player_2);
        p2_artworkSprite.sprite = character.Image;
        p2_nameText.text = character.Name;
    }

    private void Load()
    {
        Player_2 = PlayerPrefs.GetInt("Player_2");
    }

    private void Save()
    {
        PlayerPrefs.SetInt("Player_2", Player_2);
    }

    public void PlayGame() //This function loads the next scene after button is pressed
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
    }
    
}


