using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : EntityStats
{
    public int xp = 0;

    public int attack = 0;
    public int defense = 0;
    public int health = 0;
    public int luck = 0;
    public int free = 0;

    protected int[] xpLevel = null;
    // Start is called before the first frame update
    void Start()
    {
        xpLevel = new int[100];
        xpLevel[0] = 0;
        for (int i = 1; i < 100; i++)
        {
            xpLevel[i] = (int) Math.Floor((i * 300) * Math.Pow(1.5f, i / 5));
        }

        SetStats();

        hp = getHPMax();
    }

    protected override void SetStats()
    {
        int curLevel = getLevel();
        if (curLevel == 1)
        {
            attack = 2;
            defense = 2;
            health = 2;
            luck = 2;
        }
        if (curLevel <= 10)
        {
            int mod = curLevel + 1;
            attack = mod;
            defense = mod;
            health = mod;
            luck = mod;
        }
    }

    void ModXP(int xp)
    {
        int curLevel = getLevel();
        this.xp += xp;
        int nextLevel = getLevel();
        if (curLevel != nextLevel)
        {
            for (int i = curLevel; i < nextLevel; i++)
            {
                LevelUp(i);
            }
        }
    }

    void LevelUp (int level)
    {
        int curLevel = getLevel();
        if (curLevel > 10)
            luck += 4;

        SetStats();
    }

    public override int getLevel()
    {
        for (int i = 1; i <= 100; i++)
        {
            if (xp <= xpLevel[i])
                return i;
        }
        return 0;
    }

    public override int getAttack() { return attack; }
    public override int getDefense() { return defense; }
    public override int getHealth() { return health; }
    public override int getLuck() { return luck; }

    public override int getHPMax()
    {
        int hp = 100 + (getHealth() * 50) + (getHealth() * getHealth());
        return hp;
    }

    public override int BaseCrit()
    { return (int)Math.Floor(getLuck() / 5f); }
    public override int BaseCritDef()
    { return (int)Math.Floor(getLuck() / 5f); }

    public override int BaseDamage()
    { return (getAttack() * 5) + getLuck(); }
    public override int BaseDamageDef()
    { return (getDefense() * 5) + getLuck(); }

    public override void OnHealthChange()
    { }
}
