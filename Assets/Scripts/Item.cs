using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SpriteLibraryAsset = UnityEngine.U2D.Animation.SpriteLibraryAsset;

public class Item : MonoBehaviour
{
    protected GameObject player;

    public string itemType;

    public float modifier;

    public SpriteLibraryAsset spriteLibraryAsset;

    protected float pickupDistance = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
            if (player != null)
            {

                if (Vector2.Distance(transform.position, player.transform.position) <= pickupDistance)
                    UseItem(player);
            }
        //}
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
            player = other.gameObject;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
            player = null;
    }

    void UseItem(GameObject goPlayer)
    {
        PlayerStats playerStats = goPlayer.GetComponent<PlayerStats>();
        Player player = goPlayer.GetComponent<Player>();

        if (itemType == "health")
        {
            playerStats.hp = ModIntStat(playerStats.hp, modifier, playerStats.getHPMax());
        }
        else if (itemType == "weapon")
        {
            if (modifier > playerStats.weapon)
            {
                playerStats.weapon = (int) modifier;
                player.mySpriteLibs[1].spriteLibraryAsset = spriteLibraryAsset;
            }
        }
        else if (itemType == "armor")
        {
            if (modifier > playerStats.armor)
            {
                playerStats.armor = (int) modifier;
                player.mySpriteLibs[0].spriteLibraryAsset = spriteLibraryAsset;
            }
        }

        goPlayer.GetComponent<PlayerMovement>().ResetTargets();
        Destroy(this.transform.gameObject);
    }

    int ModIntStat(int stat, float modifier, int statmax = 0)
    {
        if (Math.Abs(modifier) < 1f && statmax > 0)
        {
            if (statmax > 0)
                stat += (int) Math.Round((float) statmax * modifier);
            else
                stat += (int) Math.Round((float) stat * modifier);
        }
        else
        {
            stat += (int) Mathf.FloorToInt(modifier);
        }
        if (statmax > 0)
        {
            stat = (int) Mathf.Clamp(stat, 0, statmax);
        }
        return stat;
    }
}
