using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player2 : MonoBehaviour
{
       public CharacterDatabase characterDB;
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
    public void UpdateCharacter(int Player_2)
    {
        Character character = characterDB.GetCharacter(Player_2);
        p2_artworkSprite.sprite = character.Image;

    }
    private void Load()
    {
        Player_2 = PlayerPrefs.GetInt("Player_2");
    }
}
