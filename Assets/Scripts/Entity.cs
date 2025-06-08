using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public delegate void deathAction();
    public event deathAction EventDeath;
    public void Death() { EventDeath(); }

    public delegate void respawnAction();
    public event respawnAction EventRespawn;
    public void Respawn() { EventRespawn(); }
}
