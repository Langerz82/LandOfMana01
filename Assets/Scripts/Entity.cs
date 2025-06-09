using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public delegate void deathAction(GameObject killer);
    public event deathAction EventDeath;
    public void Death(GameObject killer) { EventDeath(killer); }

    public delegate void respawnAction();
    public event respawnAction EventRespawn;
    public void Respawn() { EventRespawn(); }

    protected GameObject goRespawn;

    /*protected void SetVisible(bool visible)
    {
        Vector3 pos = this.transform.position;
        pos.z = (visible) ? 0f : -1f;
        this.transform.position = pos;
    }*/
}
