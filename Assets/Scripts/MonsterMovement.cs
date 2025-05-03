using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class MonsterMovement : MonoBehaviour
{
    [HideInInspector] public Vector2 lookDirection = Vector2.zero;
    [HideInInspector] public Vector2 moveDirection = Vector2.zero;
    public float speed = 10f;

    [HideInInspector] public CameraMMO2D cameraScript;
    [HideInInspector] public Rigidbody2D myRigidbody;
    [HideInInspector] public BoxCollider2D myCollider;

    [HideInInspector] public bool hasCollided = false;

    protected GameObject map;
    protected TileMap mapScript;

    protected Vector2[] myPath = null;
    protected int myPathIndex = 1;
    protected bool isOnPath = false;

    protected bool snapToGrid = true;

    public float aggressionRadius = 0f;
    protected float aggressionTimer = 0f;
    public float aggressionInterval = 1f;

    protected float movementTimer = 0f;
    public float movementInterval = 1f;

    protected Main mainScript;

    [HideInInspector] public string state;

    [HideInInspector] public GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        mainScript = GameObject.FindWithTag("Main").GetComponent<Main>();
        myRigidbody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>();

        state = "IDLE";

        map = getCurrentMap(transform.position);
        if (map)
        {
            mapScript = map.GetComponent<TileMap>();
        }
        else
        {
            Debug.LogError("MonsterMovement SetMap failed.");
        }
    }

    public void FollowPlayer(GameObject player)
    {
        Vector2 pos = (Vector2) transform.position;

        PlayerMovement playerMovementScript = player.GetComponent<PlayerMovement>();
        Vector3[] spots = playerMovementScript.getAdjacentTiles();
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
        if (tPath.Length > 1)
        {
            myPath = tPath;
        }
        else
        {
            ResetPath();
        }
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
                        FollowPlayer(player);
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
                FollowPlayer(target);
        }

        moveDirection = Vector2.zero;

        

        /*if (canClickMove && Input.GetMouseButtonDown(0))
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
            Debug.Log("CreatureMovement myPath:" + myPath);
        }*/
    }

    void LookAtTarget()
    {
        if (target == null)
        { return; }
        lookDirection = (target.transform.position - transform.position).normalized;
    }

    void FixedUpdate()
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
                LookAtTarget();
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

    public Vector3[] getAdjacentTiles()
    {
        Vector3 center = transform.position;
        if (myRigidbody.velocity != Vector2.zero)
        {
            center = Utils.RoundToGrid(center, myRigidbody.velocity);
        }

        List<Vector3> positions = new List<Vector3>();

        Vector2[] adjacent = {
            new Vector3(1, 0),
            new Vector3(-1, 0),
            new Vector3(0, 1),
            new Vector3(0, -1)
        };

        foreach (Vector2 vec in adjacent)
        {
            Vector2 tPos = (Vector2)center + vec;
            if (mapScript.checkWorldCollision(tPos, myCollider.size))
                continue;
            positions.Add((Vector3)tPos);
        }
        return positions.ToArray();
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
}
