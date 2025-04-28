using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public CharacterDatabase characterDB;

    public Text nameText;
    public SpriteRenderer artworkSprite;

    private int Player_1 = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!PlayerPrefs.HasKey("Player_1"))
        {
            Player_1 = 0;
        }
        
        else
        {
            Load();
        }
        UpdateCharacter(Player_1);
    }

    public void NextOption()
    {
        Player_1++;
        if (Player_1 >= characterDB.CharacterCount)
        {
            Player_1 = 0;
        }

        UpdateCharacter(Player_1);
        Save();
    }

    public void BackOption()
    {
        Player_1--;
        if (Player_1 < 0)
        {
            Player_1 = characterDB.CharacterCount - 1;
        }

        UpdateCharacter(Player_1);
        Save();
    }

    public void UpdateCharacter(int Player_1)
    {
        Character character = characterDB.GetCharacter(Player_1);
        artworkSprite.sprite = character.Image;
        nameText.text = character.Name;
    }

    private void Load()
    {
        Player_1 = PlayerPrefs.GetInt("Player_1");
    }

    private void Save()
    {
        PlayerPrefs.SetInt("Player_1", Player_1);
    }
    
}
