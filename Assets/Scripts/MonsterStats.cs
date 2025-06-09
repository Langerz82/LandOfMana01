using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStats : EntityStats
{
    protected Monster myMonster;

    public int level = 1;

    public int attack = 0;
    public int defense = 0;
    public int health = 0;
    public int luck = 0;

    public float modAttack = 1;
    public float modDefense = 1;
    public float modHealth = 1;
    public float modLuck = 1;

    // Start is called before the first frame update
    void Start()
    {
        myMonster = GetComponent<Monster>();
        myMonster.EventRespawn += Respawn;

        Respawn();
    }

    protected override void SetStats()
    {
        int curLevel = getLevel();
        attack = curLevel;
        defense = curLevel;
        health = curLevel;
        luck = curLevel;
    }

    public override int getLevel() { return level; }
    public override int getAttack() { return (int) Math.Round(attack * modAttack); }
    public override int getDefense() { return (int) Math.Round(defense * modDefense); }
    public override int getHealth() { return (int) Math.Round(health * modHealth); }
    public override int getLuck() { return (int) Math.Round(luck * modLuck); }

    public override int getHPMax()
    {
        int hp = 1;
        //int hp = 50 + (getHealth() * 35) + (getHealth() * getHealth());
        return hp;
    }

    public override int BaseCrit()
    { return (int) Math.Floor(getLuck() / 5f); }
    public override int BaseCritDef()
    { return (int) Math.Floor(getLuck() / 5f); }

    public override int BaseDamage()
    { return (getAttack() * 5) + getLuck(); }
    public override int BaseDamageDef()
    { return (getDefense() * 5) + getLuck(); }

    public override void OnHealthChange()
    { }

    public void Respawn()
    {
        SetStats();
        hp = getHPMax();
    }
}
