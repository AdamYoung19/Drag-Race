using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player1 : MonoBehaviour
{
    public CharacterDatabase characterDB;
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
    public void UpdateCharacter(int Player_1)
    {
        Character character = characterDB.GetCharacter(Player_1);
        artworkSprite.sprite = character.Image;

    }

    private void Load()
    {
        Player_1 = PlayerPrefs.GetInt("Player_1");
    }

}
