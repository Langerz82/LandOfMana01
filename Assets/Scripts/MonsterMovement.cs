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

    //protected float movementTimer = 0f;
    //public float movementInterval = 0.5f;

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
        //movementTimer += Time.deltaTime;

        if (state == "IDLE" && aggressionRadius > 0f)
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
        else if (state == "MOVING")
        {
            // This section of code drops the current path, and shortens it
            // so the entity doesnt have to move too far to the old path.
            if (target != null && myPath != null)
            {
                Vector2 endNode = myPath[myPath.Length - 1];
                float targetDist = Vector2.Distance(target.transform.position, endNode);
                float homeDist = Vector2.Distance(transform.position, endNode);
                if (targetDist > homeDist)
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
                if (dist > 1f)
                    FollowEntity(target);
            }
        }

        moveDirection = Vector2.zero;
    }
}
