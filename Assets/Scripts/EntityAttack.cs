using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Random = UnityEngine.Random;

using Debug = UnityEngine.Debug;

public class EntityAttack : MonoBehaviour
{
    protected Entity myEntity;
    protected EntityStats myStats;
    protected EntityMovement myMovement;

    public float attackInterval = 2f;
    protected float attackTimer = 0f;
    public float attackRange = 1f;

    [HideInInspector] public GameObject target;

    [HideInInspector] public bool isAttacked = false;

    // Start is called before the first frame update
    void Start()
    {
        myEntity = GetComponent<Entity>();
        myStats = GetComponent<EntityStats>();
        myMovement = GetComponent<EntityMovement>();
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

    public bool StartAttack(GameObject target)
    {
        if (target != null && !target.activeSelf)
        {
            this.target = null;
            return false;
        }
        if (target != null && target != this.transform.gameObject)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist <= attackRange)
            {
                this.target = target;
                target.GetComponent<EntityAttack>().isAttacked = true;
                return true;
            }
        }
        this.target = null;
        return false;
    }

    public void Hit(GameObject target)
    {
        EntityStats targetStats = target.GetComponent<EntityStats>();
        if (targetStats == null)
            return;

        bool isCrit = false;
        if (Random.Range(0, 100) < (myStats.BaseCrit() - targetStats.BaseCritDef()))
            isCrit = true;

        int damage = Random.Range(1, 20);
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
            target.GetComponent<Entity>().Death(this.gameObject);
            //myMovement.EntitiesInView = null;
        }
    }
}
