using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public abstract class EntityMovement : MonoBehaviour
{
    [HideInInspector] public Vector2 lookDirection = Vector2.zero;
    [HideInInspector] public Vector2 moveDirection = Vector2.zero;
    public float speed = 10f;

    protected GameObject map;
    protected TileMap mapScript;

    protected Main mainScript;

    [HideInInspector] public Rigidbody2D myRigidbody;
    [HideInInspector] public BoxCollider2D myCollider;
    [HideInInspector] protected EntityAttack myEntityAttack;

    protected Vector2[] myPath = null;
    protected int myPathIndex = 1;
    protected bool isOnPath = false;

    protected bool hasCollided = false;
    protected bool snapToGrid = true;

    [HideInInspector] public GameObject target;

    protected virtual void Start()
    {
        mainScript = GameObject.FindWithTag("Main").GetComponent<Main>();
        myRigidbody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>();
        myEntityAttack = GetComponent<EntityAttack>();
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

                if(target != null)
                {
                    float dist = Vector2.Distance(transform.position, target.transform.position);
                    if (dist <= 1f)
                        LookAtEntity(target);
                }
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
    }

    protected float getAttackRange()
    {
        EntityAttack entityAttack = GetComponent<EntityAttack>();
        if (entityAttack != null)
        {
            return entityAttack.attackRange;
        }
        return 1f;
    }

    public void FollowEntity(GameObject entity, float range = 1f)
    {
        if (entity.GetComponent<Item>() != null)
        {
            range = 0f;
        }

        Vector2 pos = (Vector2)transform.position;
        Vector2 posTarget = Utils.RoundOffToGrid(entity.transform.position);

        posTarget.y += myCollider.bounds.size.y / 2;
        Vector2[] tPath = mapScript.FindWorldPath(pos, posTarget, myCollider);
        if (tPath != null && tPath.Length > 1)
        {
            // If range > 0f traverse to nearest block within range.
            if (range > 0f)
            {
                int i = 0;
                Vector2 node2 = Vector2.zero;
                Vector2 lastNode = tPath[tPath.Length-1];
                foreach (Vector2 node in tPath)
                {
                    if (node2 != Vector2.zero)
                    {
                        Vector2 direction = (node - node2).normalized;
                        for (Vector2 n = node2; n != node; n += direction)
                        {
                            if (Vector2.Distance(n, lastNode) <= range)
                            {
                                lastNode = n;
                                break;
                            }
                        }
                    }
                    node2 = node;
                    i++;
                }
                tPath[i-1] = lastNode;
                Array.Resize(ref tPath, i);
            }
            myPath = tPath;
        }
        else
        {
            ResetPath();
        }
    }

    public void LookAtEntity(GameObject target)
    {
        if (target == null)
        { return; }
        lookDirection = (target.transform.position - transform.position).normalized;
    }

    public void ResetPath()
    {
        myPathIndex = 1;
        myPath = null;
    }


    public GameObject getCurrentMap(Vector3 position)
    {
        GameObject[] goMaps = GameObject.FindGameObjectsWithTag("Map");
        foreach (GameObject goMap in goMaps)
        {
            //BoxCollider2D collider = map.transform.Find("Collider").gameObject.GetComponent<BoxCollider2D>();
            TileMap map = goMap.GetComponent<TileMap>();
            /*if (GetComponent<Collider>() == null)
            {
                Debug.LogError("Put a BoxCollider on the Grid child object of map: " + map.name);
                return null;
            }*/
            if (map.m_Bounds.Contains(position))
                return goMap;
        }
        return null;
    }

    public virtual void SetCameraMap()
    {
        map = getCurrentMap(transform.position);
        if (map)
        {
            // Add Monster to Map Entities.
            GameObject goEntities = map.transform.Find("Entities").gameObject;
            if (goEntities != null && this.transform.parent != goEntities.transform)
                this.transform.parent = goEntities.transform;

            mapScript = map.GetComponent<TileMap>();
        }
        else
        {
            Debug.LogError("MonsterMovement SetMap failed.");
        }
    }
}
