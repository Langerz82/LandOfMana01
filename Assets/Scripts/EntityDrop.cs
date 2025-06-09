using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

[System.Serializable]
public class EntityDropItem
{
    [SerializeField] public GameObject gameObject;
    [SerializeField] public int chance;

    public void Drop(GameObject gameObject, int chance)
    {
        this.gameObject = gameObject;
        this.chance = chance;
    }
}

public class EntityDrop : MonoBehaviour
{
    protected Entity myEntity;
    public EntityDropItem[] DropList;

    // Start is called before the first frame update
    void Start()
    {
        myEntity = GetComponent<Entity>();

        myEntity.EventDeath += Death;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Death(GameObject killer)
    {
        GameObject go = null;
        int rand = Random.Range(0, 1000);
        foreach (var item in DropList)
        {
            if (rand < item.chance)
            {
                go = item.gameObject;
                break;
            }
            rand -= item.chance;
        }
        if (go != null)
        {
            GameObject item = Instantiate(go, transform.position, transform.rotation);
            this.GetComponent<EntityMovement>().mapScript.AddEntity(item);
        }
    }
}

