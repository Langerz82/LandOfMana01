using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector] public Vector2 lookDirection = Vector2.zero;
    [HideInInspector] public Vector2 moveDirection = Vector2.zero;
    public float speed = 10f;

    [HideInInspector] public CameraMMO2D cameraScript;
    [HideInInspector] public GameObject playerCamera;
    [HideInInspector] public Rigidbody2D myRigidbody;
    [HideInInspector] public BoxCollider2D myCollider;
    //[HideInInspector] public Camera myCamera;

    [HideInInspector] public bool hasCollided = false;
    [HideInInspector] protected Vector3 prevMove;
    [HideInInspector] protected Vector2 lastMove;

    protected GameObject map;
    protected TileMap mapScript;

    protected bool canClickMove = false;

    protected Vector2[] myPath = null;
    protected int myPathIndex = 1;
    protected bool isOnPath = false;

    protected bool snapToGrid = true;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GameObject.FindWithTag("MainCamera");
        if (playerCamera)
            cameraScript = playerCamera.GetComponent<CameraMMO2D>();
        else
            Debug.LogError("MainCamera not found.");

        myRigidbody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>();
        //myCamera = playerCamera.GetComponent<cameraScript>();

        SetPlayerMap();
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = Vector2.zero;

        Vector3 pos = transform.position;

        canClickMove = false;
        if (myPath == null && pos.x % 0.5 == 0 && pos.y % 0.5 == 0)
            canClickMove = true;

        if (canClickMove && Input.GetMouseButtonDown(0))
        {
            Vector2 posTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            posTarget.y += myCollider.bounds.size.y / 2;
            Vector2[] tPath = mapScript.FindWorldPath((Vector2)pos, (Vector2)posTarget, myCollider);
            if (tPath.Length > 1)
            {
                myPath = tPath;
            }
            else
            {
                ResetPath();
            }
            Debug.Log("PlayerMovement myPath:" + myPath);
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

    void FixedUpdate()
    {
        Vector3 pos = transform.position;

        if (myPath != null && myPath.Length > 1)
        {
            Vector2 dest = (Vector2)myPath[myPathIndex];
            Vector2 dir = ((Vector2)dest - (Vector2)pos).normalized;
            moveDirection = dir;
            lastMove = (Vector2)pos;

            float nextDist = speed * Time.unscaledDeltaTime;
            Vector2 nextPosition = moveDirection * nextDist + (Vector2)pos;
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
                transform.position = Utils.RoundToGrid(pos, myRigidbody.velocity);
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
                Vector3 targetPos = Utils.RoundToGrid(pos, velocity);
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
    }


    void ResetPath()
    {
        myPathIndex = 1;
        myPath = null;
    }

    protected Vector3 clampPlayer(Vector3 destination)
    {
        Vector3 offset = GetComponent<BoxCollider2D>().bounds.extents;

        return cameraScript.ClampMapWithOffset(destination, offset);
    }

    public void SetPlayerMap()
    {
        map = getCurrentMap(transform.localPosition);
        if (map)
        {
            cameraScript.SetCameraBounds(map);
            mapScript = map.GetComponent<TileMap>();
        }
        else
        {
            Debug.LogError("PlayerMovement SetPlayerMap failed.");
        }
    }

    public void WarpPlayer(Vector3 position)
    {
        myRigidbody.transform.position = position;
        SetPlayerMap();
    }

    public GameObject getCurrentMap(Vector3 position)
    {
        GameObject[] goMaps = GameObject.FindGameObjectsWithTag("Map");
        foreach (GameObject map in goMaps)
        {
            BoxCollider2D collider = map.transform.GetChild(1).GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                Debug.LogError("Put a BoxCollider on the Grid child object of map: " + map.name);
                return null;
            }
            if (collider.bounds.Contains(position))
                return map;
        }
        return null;
    }

    /*
    // Not needed for now.
    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("OnTriggerEnter2D - collider name:" + collider);
        if (collider.transform.parent != null)
        {
            Debug.Log("layer" + collider.transform.parent.gameObject.layer);
            if (collider.transform.parent.gameObject.layer == LayerMask.NameToLayer("Collision"))
            {
            }
        }
    }*/
}
