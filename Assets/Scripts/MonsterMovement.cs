using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class MonsterMovement : EntityMovement
{
    public float aggressionRadius = 0f;
    protected float aggressionTimer = 0f;
    public float aggressionInterval = 1f;

    protected Vector3 vecHome;
    public float m_ReturnHomeDistance = 10f;

    protected Monster myMonster;

    // Start is called before the first frame update
    protected override void Start()
    {
        myMonster = GetComponent<Monster>();
        myMonster.EventRespawn += OnRespawn;

        base.Start();

        SetCameraMap();

        vecHome = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (myMonster.state == "DEAD")
            return;

        Vector3 pos = transform.position;

        if (target != null && target.GetComponent<EntityAttack>() != null)
        {
            if (myEntityAttack.StartAttack(target))
                LookAtEntity(target);
        }
        else
        {
            myEntityAttack.StartAttack(null);
        }

        if (myMonster.state == "IDLE")
        {
            aggressionTimer += Time.deltaTime;
            bool isAttack = (aggressionRadius > 0f || myEntityAttack.isAttacked);

            if (isAttack && aggressionTimer > aggressionInterval)
            {
                // Perform check.
                foreach (GameObject player in mainScript.players)
                {
                    float dist = Vector3.Distance(pos, player.transform.position);
                    if (dist <= aggressionRadius || myEntityAttack.isAttacked)
                    {
                        target = player;
                        myMonster.state = "MOVING";
                        FollowEntity(player, getAttackRange());
                        break;
                    }
                }
                aggressionTimer = 0;
            }
        }
        else if (myMonster.state == "MOVING")
        {
            // This section of code drops the current path, and shortens it
            // so the entity doesnt have to move too far to the old path.
            if (target != null && myPath != null)
            {
                Vector2 endNode = myPath[myPath.Length - 1];
                float targetDist = Vector2.Distance(target.transform.position, endNode);
                float endDist = Vector2.Distance(transform.position, endNode);
                if (targetDist > endDist)
                {
                    Vector2 dest = (Vector2)myPath[myPathIndex];
                    moveDirection = ((Vector2)dest - (Vector2)pos).normalized;
                    Vector3 endPos = Utils.RoundNextPosToGrid(pos, moveDirection);
                    myPath[myPathIndex] = endPos;
                    Array.Resize(ref myPath, myPathIndex + 1);
                }
            }

            if (target != null && myPath == null)
            {
                float dist = Vector3.Distance(pos, target.transform.position);
                if (dist == 0f || dist > 1f)
                    FollowEntity(target, getAttackRange());
            }

            float homeDist = Vector3.Distance(vecHome, transform.position);
            if(homeDist >= m_ReturnHomeDistance)
            {
                myEntityAttack.StartAttack(null);
                target = null;
                MoveToPosition(vecHome);
            }
        }

        moveDirection = Vector2.zero;
    }

    void OnRespawn()
    {
        myEntityAttack.StartAttack(null);
        target = null;
    }
}
