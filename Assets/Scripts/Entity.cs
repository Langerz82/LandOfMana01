using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public delegate void deathAction();
    public event deathAction EventDeath;
    public void OnDeath() { EventDeath(); }
}
