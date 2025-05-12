using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

using SuperMap = SuperTiled2Unity.SuperMap;
using Random = UnityEngine.Random;
using Pathfinder;
using System;
using System.Collections.Specialized;
using System.IO;

public class TileMap : MonoBehaviour
{
    [HideInInspector] protected SuperMap superMapScript;

    protected float agentRadius = 0.5f;
    protected LayerMask collisionMask;

//    protected PathFinder.Map mapPath = new PathFinder.Map();

    //protected int width = GetComponent<SuperMap>().m_Width;
    public int mWidth = 0;
    public int mHeight = 0;

    protected bool[,] mapCollision = null;

    protected List<Vector2> collisionGrid = new List<Vector2>();
    protected Vector2 colPosition;

    private void OnDrawGizmos()
    {
        if (superMapScript == null)
            return;

        int width = superMapScript.m_Width;
        int height = superMapScript.m_Height;

        Gizmos.color = new Color(0f, 1f, 0f);
        Vector2 size = new Vector2(1, 1);

        foreach (Vector2 pos in collisionGrid)
            Gizmos.DrawWireCube(pos, size);

        Gizmos.color = new Color(1f, 0f, 0f);
        size = new Vector2(0.9f, 0.9f);
        Gizmos.DrawWireCube(colPosition, size);
    }

    public void SetVisible(bool visible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = visible;
    }

    void Awake()
    {
        SetVisible(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        superMapScript = GetComponent<SuperMap>();
        collisionMask = LayerMask.NameToLayer("Collision"); // 1 << LayerMask.NameToLayer("Collision");
        int width = superMapScript.m_Width;
        int height = superMapScript.m_Height;

        mapCollision = new bool[width, height];
        Vector2 size = new Vector2(agentRadius, agentRadius);
        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                mapCollision[j, i] = checkGridCollision(new Vector2(j, i), size, true);
            }
        }

        /*for (int i = 0; i < 10; ++i)
        {
            Vector2 start = new Vector2(Random.Range(0, width), Random.Range(0, height));
            Vector2 end = new Vector2(Random.Range(0, width), Random.Range(0, height));
            FindPath(start, end);
        }*/
    }

    public Vector2[] FindWorldPath(Vector2 start, Vector2 end, Collider2D collider)
    {
        Vector2 pos = (Vector2)transform.position;
        start = Vector2Int.FloorToInt((Vector2) start - pos);
        end = Vector2Int.FloorToInt((Vector2) end - pos);
        start.y = Math.Abs(start.y);
        end.y = Math.Abs(end.y);
        List<Vector2> resPath = FindPath(start, end);
        if (resPath.Count > 1)
        {
            Vector2[] path = ConvertPathToWorldPath(resPath);
            /*for (int i = 0; i < path.Length; ++i)
            {
                path[i][1] -= collider.bounds.center.y;
            }*/
            for (int i = 0; i < path.Length; ++i)
            {
                path[i].x += agentRadius;
                //path[i].y -= collider.bounds.size.y / 2; 
            }
            DrawPath(path);
            return path;
        }
        return null;
    }

    public Vector2[] ConvertPathToWorldPath (List<Vector2> vectors)
    {
        Vector2 pos = (Vector2)transform.position;
        Vector2[] fPath = new Vector2[vectors.Count];
        int i = 0;
        foreach (Vector2 vec in vectors)
        {
            fPath[i++] = new Vector2(pos.x + vec.x, pos.y - vec.y);
        }
        return fPath;
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        Pathfinder.Node startNode = new Pathfinder.Node((int) start.x, (int)start.y);
        Pathfinder.Node endNode = new Pathfinder.Node((int)end.x, (int)end.y);

        List<Pathfinder.Node> path = Pathfinder.AStar.GetInstance.FindPath(startNode, endNode, mapCollision);
        Debug.Log("path:" + path);

        path = Pathfinder.AStar.GetInstance.SimplifyNodeList(path);
        Debug.Log("newPath:" + path);

        List<Vector2> vectors = new List<Vector2>();
        foreach(Pathfinder.Node node in path)
        {
            vectors.Add(new Vector2(node.X, node.Y));
        }
        return vectors;
    }

    void DrawPath(Vector2[] vectors)
    {
        //Vector2 pos = transform.position;//' + agentRadius;
        //float y = transform.position.y;// - agentRadius;

        Vector2 tVec = Vector2.zero;
        foreach (Vector2 vec in vectors)
        {
            if (tVec != Vector2.zero)
            {
                Vector3 start = new Vector3(tVec.x, tVec.y, 0f);
                Vector3 end = new Vector3(vec.x, vec.y, 0f);
                Debug.DrawLine(start, end, Color.red, 60000);
            }
            tVec = vec;
        }
    }

    public bool checkWorldCollision(Vector2 position, Vector2 size, bool drawGizmo = false)
    {
        //Debug.Log("checkCollision - pos: " + position);
        //Vector2 size = new Vector2(agentRadius, agentRadius);
        //Vector2 boxSize = new Vector2(size.x, size.y);
        Collider2D collider = Physics2D.OverlapBox(position, size, 0f, 1 << collisionMask);

        if (collider != null)
        {
            if (drawGizmo)
            {
                collisionGrid.Add(position);
            } else
            {
                colPosition = position;
            }

            Debug.Log("checkCollision - pos: " + position);
            Debug.Log("collision found.");
        }
        return (collider != null);
    }

    public bool checkGridCollision(Vector2 position, Vector2 size, bool drawGizmo = false)
    {
        float x = transform.position.x;
        float y = transform.position.y;
        Vector2 pos = new Vector2(x + position.x + agentRadius, y - position.y - agentRadius);
        //size = new Vector2(agentRadius, agentRadius);
        return checkWorldCollision(pos, size, drawGizmo);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3[] getAdjacentTiles(GameObject entity)
    {
        Vector3 center = entity.transform.position;
        Rigidbody2D myRigidbody = entity.GetComponent<Rigidbody2D>();

        if (myRigidbody != null && myRigidbody.velocity != Vector2.zero)
        {
            center = Utils.RoundNextPosToGrid(center, myRigidbody.velocity);
        }

        List<Vector3> positions = new List<Vector3>();

        Vector2[] adjacent = {
            new Vector3(1, 0),
            new Vector3(-1, 0),
            new Vector3(0, 1),
            new Vector3(0, -1)
        };

        Collider2D myCollider = entity.GetComponent<Collider2D>();
        Vector2 size = (myCollider != null ? myCollider.bounds.size : new Vector2(1f, 1f));
        foreach (Vector2 vec in adjacent)
        {
            Vector2 tPos = (Vector2)center + vec;
            if (checkWorldCollision(tPos, size))
                continue;
            positions.Add((Vector3)tPos);
        }
        return positions.ToArray();
    }
}
