// Simple MMO camera that always follows the player.
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;

using TilemapRenderer = UnityEngine.Tilemaps.TilemapRenderer;

using Debug = UnityEngine.Debug;

public class CameraMMO2D : MonoBehaviour
{

    [Header("Target Follow")]
    public Transform target;

    // the target position can be adjusted by an offset in order to foucs on a
    // target's head for example
    public Vector2 offset = Vector2.zero;

    // smooth the camera movement
    [Header("Dampening")]
    public float damp = 1f;

    // BEGIN MOD - JL - 8/6/24, 30/6/24
    [HideInInspector] public BoxCollider2D myCollider;
    public GameObject cameraBounds;

    // END MOD

    private Camera myCamera;

    void Start()
    {
        myCamera = GetComponent<Camera>();
        SetCameraBounds(cameraBounds);
    }

    void LateUpdate()
    {
        if (!target) return;

        // calculate goal position
        Vector2 goal = (Vector2)target.transform.position + offset;
        //goal.x += offset.x;
        //goal.y += offset.y;

        // BEGIN MOD - JL - 6/6/24
        float precision = 64f;
        float toleranceX = precision / Screen.width;
        float toleranceY = precision / Screen.height;
        Vector2 diff = (Vector2)transform.position - (Vector2)goal;

        Vector3 position = new Vector3(goal.x, goal.y, transform.position.z);

        /*if (Math.Abs(diff.x) > toleranceX || Math.Abs(diff.y) > toleranceY)
        {
            // interpolate
            position = Vector2.Lerp(transform.position, goal, Time.deltaTime * damp);
        }*/

        if (myCollider)
        {
            //Debug.Log("position:" + position);
            Vector3 pos = ClampCamera(position);
            if (pos != Vector3.zero && pos != position)
            {
                position.x = pos.x;
                position.y = pos.y;
            }
        }

        // END MOD
        // convert to 3D but keep Z to stay in front of 2D plane
        transform.position = position;
    }

    public Vector3 ClampCamera(Vector3 position)
    {
        Vector2 camSize = new Vector2(
            myCamera.orthographicSize * myCamera.aspect,
            myCamera.orthographicSize);
        //Debug.Log("ClampMap - camSize:" + camSize);

        return ClampMapWithOffset(myCollider, camSize, position);
    }

    public Vector3 ClampMapWithOffset(Vector3 position, Vector2 offset)
    {
        return ClampMapWithOffset(myCollider, offset, position);
    }

    // BEGIN MOD - JL - 6/6/24
    // Exact Copy from function in CameraMMO2D.
    protected Vector3 ClampMapWithOffset(BoxCollider2D boxCollider, Vector2 offset, Vector3 position)
    {
        Vector2 clipMin = boxCollider.bounds.min;
        Vector2 clipMax = boxCollider.bounds.max;
        if (offset != Vector2.zero)
        {
            //Debug.Log("ClampMap - mapMin:" + mapMin);
            //Debug.Log("ClampMap - mapMax:" + mapMax);

            clipMin = new Vector2(clipMin.x + offset.x, clipMin.y + offset.y);
            clipMax = new Vector2(clipMax.x - offset.x, clipMax.y - offset.y);
            //Debug.Log("ClampMap - clipMin:" + clipMin);
            //Debug.Log("ClampMap - clipMax:" + clipMax);
            //Debug.Log("ClampMap - position:" + position);
        }

        float newX = Mathf.Clamp(position.x, clipMin.x, clipMax.x);
        float newY = Mathf.Clamp(position.y, clipMin.y, clipMax.y);
        //Debug.Log("ClampMap - new_position:" + (new Vector2(newX, newY)));
        return new Vector3(newX, newY, position.z);
    }
    // END MOD

    // BEGIN MOD - JL - 8/6/24
    // Switch Camera Position to target immediately.
    public void MoveCamera(Vector3 newPosition)
    {
        Debug.Log("MoveCamera - called:" + newPosition);
        // convert to 3D but keep Z to stay in front of 2D plane
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }
    // END MOD

    // BEGIN MOD - JL - 8/6/24
    public void SetCameraBounds(GameObject bounds)
    {
        if (bounds)
        {
            cameraBounds = bounds;
            myCollider = cameraBounds.transform.GetChild(0).GetComponent<BoxCollider2D>();
            if (myCollider == null)
            { 
                Debug.LogWarning("Make sure BoxCollider2D is on tilemap Grid Object.");
            }
        }
        else
        {
            cameraBounds = null;
            myCollider = null;
        }
    }
    // END MOD

}
