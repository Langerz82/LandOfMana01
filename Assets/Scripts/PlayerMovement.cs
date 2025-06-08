using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class PlayerMovement : EntityMovement
{
    [HideInInspector] public CameraMMO2D cameraScript;
    //[HideInInspector] public Rigidbody2D myRigidbody;
    //[HideInInspector] public BoxCollider2D myCollider;

    //[HideInInspector] public bool hasCollided = false;


    protected bool canClickMove = false;

    //protected Vector2[] myPath = null;
    //protected int myPathIndex = 1;
    //protected bool isOnPath = false;

    //protected bool snapToGrid = true;

    //protected GameObject target;

    protected int targetIndex = 0;
    protected GameObject[] EntitiesInView = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        cameraScript = GameObject.FindWithTag("MainCamera").GetComponent<CameraMMO2D>();
        if (cameraScript == null)
        {
            Debug.LogError("MainCamera not found.");
        }

        base.Start();
        SetCameraMap();
    }

    public int ClosestEntities(GameObject s1, GameObject s2)
    {
        float d1 = Vector3.Distance(s1.transform.position, transform.position);
        float d2 = Vector3.Distance(s2.transform.position, transform.position);
        if (d1 < d2)
            return -1;
        if (d1 > d2)
            return 1;
        return 0;
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = Vector2.zero;

        Vector3 pos = transform.position;

        canClickMove = false;
        if (myPath == null && pos.x % 0.5 == 0 && pos.x != 1 && pos.y % 0.5 == 0)
            canClickMove = true;

        if (canClickMove && Input.GetMouseButtonDown(0))
        {
            Vector2 posTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Collider2D collider = Physics2D.OverlapBox(posTarget, myCollider.size, 0f, 1 << LayerMask.NameToLayer("Entities"));
            if (collider != null)
            {
                if (target == collider.gameObject)
                {
                    FollowEntity(collider.gameObject, getAttackRange());
                }
                else
                {
                    target = collider.gameObject;
                }                
            }
            else
            {
                posTarget.y += myCollider.bounds.size.y / 2;
                Vector2[] tPath = mapScript.FindWorldPath((Vector2)pos, (Vector2)posTarget, myCollider);
                if (tPath != null && tPath.Length > 1)
                {
                    myPath = tPath;
                }
                else
                {
                    ResetPath();
                }
                target = null;
            }
            Debug.Log("PlayerMovement myPath:" + myPath);
        }

        if (myRigidbody.velocity != Vector2.zero)
        {
            EntitiesInView = null;
            targetIndex = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (target == null)
            {
                if (EntitiesInView == null)
                {
                    GameObject[] exclude = { transform.gameObject };
                    EntitiesInView = cameraScript.GetEntitiesInView(exclude);
                    Array.Sort(EntitiesInView, ClosestEntities);
                }
                if (EntitiesInView.Length > 0 && targetIndex < EntitiesInView.Length)
                {
                    target = EntitiesInView[targetIndex];
                    targetIndex = (targetIndex + 1) % EntitiesInView.Length;
                }
            }
            else
            {
                FollowEntity(target, getAttackRange());
            }
        }

        if (target != null && target.GetComponent<EntityAttack>() != null)
        {
            if (myEntityAttack.StartAttack(target))
                LookAtEntity(target);
        } else
        {
            myEntityAttack.StartAttack(null);
        }

        // I had to remove it to let the grid rounding work properly. 
        // velocity is checked so the player cant change direction without finishing on the grid.
        // I need to do a workaround so both cases will work properly.
        // Velocity on the opposite axis is checked to make sure the entity
        // is on the grid before moving again in a different direction.
        if (myPath == null)
        {
            // NOTE - Make sure only one key will work at a time.
            bool noVec = (myRigidbody.velocity == Vector2.zero);
            if (Input.GetAxis("Vertical") > 0 && (noVec || myRigidbody.velocity.y > 0))
            {
                moveDirection.y = 1;
            }
            else if (Input.GetAxis("Vertical") < 0 && (noVec || myRigidbody.velocity.y < 0))
            {
                moveDirection.y = -1;
            }
            else if (Input.GetAxis("Horizontal") < 0 && (noVec || myRigidbody.velocity.x < 0))
            {
                moveDirection.x = -1;
            }
            else if (Input.GetAxis("Horizontal") > 0 && (noVec || myRigidbody.velocity.x > 0))
            {
                moveDirection.x = 1;
            }
        }
    }

    protected Vector3 clampPlayer(Vector3 destination)
    {
        Vector3 offset = GetComponent<BoxCollider2D>().bounds.extents;

        return cameraScript.ClampMapWithOffset(destination, offset);
    }

    public override void SetCameraMap()
    {
        map = getCurrentMap(transform.position);
        if (map)
        {
            mapScript = map.GetComponent<TileMap>();
            if (mapScript != null)
            {
                mapScript.SetVisible(true);
            }
            else
            {
                Debug.LogError("PlayerMovement SetPlayerMap mapScript not found.");
            }
            cameraScript.SetCameraBounds(map);

            // Add Player to Map Entities.
            GameObject goEntities = map.transform.Find("Entities").gameObject;
            if (goEntities != null && this.transform.parent != goEntities.transform)
                this.transform.parent = goEntities.transform;
        }
        else
        {
            Debug.LogError("PlayerMovement SetPlayerMap failed.");
        }
    }

    public void WarpPlayer(Vector3 position)
    {
        myRigidbody.transform.position = position;
        SetCameraMap();
    }
}
