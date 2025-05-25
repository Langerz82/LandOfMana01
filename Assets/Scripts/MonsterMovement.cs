using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class MonsterMovement : EntityMovement
{
    //protected bool hasCollided = false;

    //protected GameObject map;
    //protected TileMap mapScript;

    //protected bool snapToGrid = true;

    public float aggressionRadius = 0f;
    protected float aggressionTimer = 0f;
    public float aggressionInterval = 1f;

    protected float movementTimer = 0f;
    public float movementInterval = 1f;

    [HideInInspector] public string state;

    //[HideInInspector] public GameObject target;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        /*cameraScript = GameObject.FindWithTag("MainCamera").GetComponent<CameraMMO2D>();
        if (cameraScript == null)
        {
            Debug.LogError("MainCamera not found.");
        }*/

        //base.Init();
        state = "IDLE";

        
    }



    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        aggressionTimer += Time.deltaTime;
        movementTimer += Time.deltaTime;

        if (state=="IDLE" && aggressionRadius > 0f)
        {
            
            if (aggressionTimer > aggressionInterval)
            {
                // Perform check.
                foreach (GameObject player in mainScript.players)
                {
                    float dist = Vector3.Distance(pos, player.transform.position);
                    if (dist <= aggressionRadius)
                    {
                        target = player;
                        state = "MOVING";
                        FollowEntity(player);
                        break;
                    }
                }
                aggressionTimer = 0;
            }
        }
        else if (state=="MOVING" && movementTimer > movementInterval)
        {
            float dist = Vector3.Distance(pos, target.transform.position);
            if (target != null && myPath == null && dist > 1)
                FollowEntity(target);
        }

        moveDirection = Vector2.zero;
    }

    /*void FixedUpdate()
    {
        Vector3 pos = transform.position;

        if (myPath != null && myPath.Length > 1)
        {
            Vector2 dest = (Vector2)myPath[myPathIndex];
            moveDirection = ((Vector2)dest - (Vector2)pos).normalized;

            float nextDist = speed * Time.unscaledDeltaTime;
            Vector2 nextPosition = (moveDirection * nextDist) + (Vector2)pos;
            if (Vector2.Distance(nextPosition, dest) <= nextDist)
            {
                myPathIndex++;
                myRigidbody.transform.position = dest;
                moveDirection = Vector2.zero;
                isOnPath = true;
            }
            if (myPathIndex >= myPath.Length)
            {
                ResetPath();
                LookAtEntity(target);
            }
        }
        else
        {
            ResetPath();
        }

        if (moveDirection != Vector2.zero)
        {
            lookDirection = moveDirection;
        }

        if (myPath == null && (myRigidbody.velocity != Vector2.zero || moveDirection != Vector2.zero))
        {
            Vector2 velocity = (moveDirection != Vector2.zero) ? moveDirection * speed : myRigidbody.velocity;
            Vector2 nextOffset = velocity * Time.deltaTime; // * (Vector2.one / myCollider.size);
            Vector2 center = nextOffset + (Vector2)pos + myCollider.offset;
            if (mapScript.checkWorldCollision(center, myCollider.size))
            {
                hasCollided = true;
                snapToGrid = false;

                // Snap to the grid.
                // TODO - Is buggy grid gets rounded into collision.
                //transform.position = Utils.RoundToGrid(pos, velocity);
                transform.position = Utils.RoundOffToGrid(pos);
            }
        }

        if (hasCollided || isOnPath)
        {
            moveDirection = Vector2.zero;
            myRigidbody.velocity = Vector2.zero;
            hasCollided = false;
            isOnPath = false;
        }
        else
        {
            bool isMoving = moveDirection != Vector2.zero;
            if (isMoving)
            {
                myRigidbody.velocity = moveDirection * speed;
            }

            // Round the movement off.
            if (!isMoving && myRigidbody.velocity != Vector2.zero)
            {
                Vector2 velocity = myRigidbody.velocity;
                Vector3 targetPos = Utils.RoundNextPosToGrid(pos, velocity);
                float tx = Math.Abs(pos.x - targetPos.x);
                float ty = Math.Abs(pos.y - targetPos.y);
                float speedDist = speed * Time.unscaledDeltaTime;
                if ((velocity.x != 0 && tx <= speedDist) || (velocity.y != 0 && ty <= speedDist))
                {
                    if (snapToGrid)
                        myRigidbody.transform.position = targetPos;
                    myRigidbody.velocity = Vector2.zero;
                    moveDirection = Vector2.zero;
                }
            }
        }
        snapToGrid = true;
    }*/
}
