using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityStats : MonoBehaviour
{
    public int hp = 0;

    protected abstract void SetStats();

    public abstract int getLevel();

    public abstract int getAttack();
    public abstract int getDefense();
    public abstract int getHealth();
    public abstract int getLuck();

    public abstract int getHPMax();

    public abstract int BaseCrit();
    public abstract int BaseCritDef();
    public abstract int BaseDamage();
    public abstract int BaseDamageDef();

}
