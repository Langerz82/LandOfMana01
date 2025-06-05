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

    public SpriteLibraryAsset spriteLibAsset;

    protected float pickupDistance = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            if (Vector2.Distance(transform.position, player.transform.position) < pickupDistance)
                UseItem(player);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            player = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            player = null;
    }

    void UseItem(GameObject player)
    {
        EntityStats playerStats = player.GetComponent<EntityStats>();

        if (itemType == "health")
        {
            playerStats.hp = ModIntStat(playerStats.hp, modifier, playerStats.getHPMax());
        }
        else if (itemType == "weapon")
        {
            // TODO
        }
        else if (itemType == "armor")
        {
            // TODO
        }
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
