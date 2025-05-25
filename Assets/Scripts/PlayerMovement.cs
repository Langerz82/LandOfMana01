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


        /*mainScript = GameObject.FindWithTag("Main").GetComponent<Main>();
        myRigidbody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>();

        SetCameraMap();*/

        //base.Init();
    }

    /*public void FollowEntity(GameObject entity)
    {
        Vector2 pos = (Vector2)transform.position;

        Vector3[] spots = mapScript.getAdjacentTiles(entity);
        int i = 0;
        float shortestDist = 0;
        int shortestIndex = 0;
        foreach (Vector3 spot in spots)
        {
            float dist = Vector2.Distance(pos, (Vector2)spot);
            if (shortestDist == 0f || dist < shortestDist)
            {
                shortestIndex = i;
                shortestDist = dist;
            }
            i++;
        }

        Vector2 posTarget = spots[shortestIndex];
        posTarget.y += myCollider.bounds.size.y / 2;
        Vector2[] tPath = mapScript.FindWorldPath(pos, posTarget, myCollider);
        if (tPath != null && tPath.Length > 1)
        {
            myPath = tPath;
        }
        else
        {
            ResetPath();
        }
    }*/

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

            Collider2D collider = Physics2D.OverlapBox(posTarget, myCollider.size, 0f, 1 << LayerMask.NameToLayer("Monsters"));
            if (collider != null)
            {
                if (target == collider.gameObject)
                {
                    FollowEntity(collider.gameObject);
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
                target = EntitiesInView[targetIndex];
                targetIndex = (targetIndex + 1) % EntitiesInView.Length;
            }
            else
            {
                FollowEntity(target);
            }
        }

        if (target != null)
        {
            float dist = Vector3.Distance(myCollider.transform.position, target.transform.position);
            if (dist <= 1f)
            {
                LookAtEntity(target);
                GetComponent<EntityAttack>().target = target;
            }
            else
            {
                GetComponent<EntityAttack>().target = null;
            }
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
            ResetPath();

        // This section of code makes sure the player does not go outside the map bounds.
        if (cameraScript.cameraBounds != null)
        {
            Vector3 dest = clampPlayer(pos);
            if (dest != pos)
            {
                hasCollided = true;
            }
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


    /*void ResetPath()
    {
        myPathIndex = 1;
        myPath = null;
    }*/

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

    /*public GameObject getCurrentMap(Vector3 position)
    {
        GameObject[] goMaps = GameObject.FindGameObjectsWithTag("Map");
        foreach (GameObject map in goMaps)
        {
            BoxCollider2D collider = map.transform.Find("Collider").gameObject.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                Debug.LogError("Put a BoxCollider on the Grid child object of map: " + map.name);
                return null;
            }
            if (collider.bounds.Contains(position))
                return map;
        }
        return null;
    }*/


}
