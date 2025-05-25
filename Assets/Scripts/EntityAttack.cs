using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Random = UnityEngine.Random;

using Debug = UnityEngine.Debug;

public class EntityAttack : MonoBehaviour
{
    public float attackInterval = 2f;
    protected float attackTimer = 0f;
    public float attackRange = 1f;

    [HideInInspector] public GameObject target;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer > attackInterval)
            {
                if (Vector3.Distance(transform.position, target.transform.position) <= attackRange)
                {
                    Hit(target);
                }
                attackTimer = 0f;
            }
        }
    }

    public void Hit(GameObject target)
    {
        //GetComponent<EntityMovement>().LookAtEntity(target);

        EntityStats myStats = GetComponent<EntityStats>();
        EntityStats targetStats = target.GetComponent<EntityStats>();

        bool isCrit = false;
        if (Random.Range(0, 100) < (myStats.BaseCrit() - targetStats.BaseCritDef()))
            isCrit = true;

        int damage = Random.Range(1, 10);
        damage += myStats.BaseDamage() - targetStats.BaseDamageDef();
        damage = Math.Max(damage, 0);

        if (isCrit)
            damage *= 2;

        Debug.Log("damage:" + damage);
        targetStats.hp -= damage;
        if (targetStats.hp <= 0)
        {
            targetStats.hp = 0;
            // DIE.
            Debug.Log("target died.");
            Destroy(target);
        }
    }

}
