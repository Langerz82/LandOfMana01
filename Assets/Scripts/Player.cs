using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;

using SpriteLibrary = UnityEngine.U2D.Animation.SpriteLibrary;
using SpriteLibraryAsset = UnityEngine.U2D.Animation.SpriteLibraryAsset;

using Debug = UnityEngine.Debug;
using System.Linq;

public class Player : MonoBehaviour
{
    [HideInInspector] public Vector2 lookDirection = Vector2.zero;
    [HideInInspector] public Vector2 moveDirection = Vector2.zero;
    public float speed = 1 / 16f;

    [HideInInspector] public CameraMMO2D cameraScript;
    [HideInInspector] public GameObject playerCamera;
    [HideInInspector] public Rigidbody2D myRigidBody;

    [HideInInspector] public string state;

    protected Animator[] mAnimators = new Animator[2];
    protected SpriteLibrary[] mySpriteLibs = new SpriteLibrary[2];

    protected bool hasCollided = false;
    protected Vector3 prevMove;
    //[HideInInspector] protected GameObject goMain;
    //[HideInInspector] protected Main mainScript;
    //[HideInInspector] public GameObject[] goMaps;

    // Start is called before the first frame update
    void Start()
    {
        //goMain = GameObject.FindWithTag("Main");
        //mainScript = goMain.GetComponent<mainScript>();

        playerCamera = GameObject.FindWithTag("MainCamera");
        if (playerCamera)
            cameraScript = playerCamera.GetComponent<CameraMMO2D>();
        else
            Debug.LogError("MainCamera not found.");

        // BEGIN MOD - JL - 5/6/24 - 12/6/24
        //mAnimators = new Animator[2];
        //mySpriteLibs = new SpriteLibrary[2];
        myRigidBody = GetComponent<Rigidbody2D>();

        Transform childTF = transform.GetChild(0).Find("sprites").Find("sprite_body");
        if (childTF)
        {
            Debug.Log("childTF - found");
            mAnimators[0] = GetComponent<Animator>();
            if (!mAnimators[0]) mAnimators[0] = childTF.GetComponent<Animator>();
            if (!mAnimators[0]) Debug.Log("Player - mAnimators[0] not found.");
            mySpriteLibs[0] = childTF.GetComponent<SpriteLibrary>();
            if (!mySpriteLibs[0]) Debug.Log("BODY SPRITELIB NOT FOUND.");
        }

        childTF = transform.GetChild(0).Find("sprites").Find("sprite_weapon");
        if (childTF)
        {
            Debug.Log("childTF2 - found");
            mAnimators[1] = GetComponent<Animator>();
            if (!mAnimators[1]) mAnimators[1] = childTF.GetComponent<Animator>();
            if (!mAnimators[1]) Debug.Log("Player - mAnimators[1] not found.");
            mySpriteLibs[1] = childTF.GetComponent<SpriteLibrary>();
            if (!mySpriteLibs[1]) Debug.Log("WEAPON SPRITELIB NOT FOUND.");
        }

        SetPlayerMap();
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = Vector2.zero;

        Vector3 destination = myRigidBody.transform.position;

        // NOTE - Make sure only one key will work at a time.
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveDirection.y = 1;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveDirection.y = -1;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection.x = -1;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection.x = 1;
        }

        if (moveDirection != Vector2.zero)
        {
            lookDirection = moveDirection;
        }

        //bool isMoving = movement.IsMoving() && state != "CASTING" && !mountControl.IsMounted();
        bool isMoving = moveDirection != Vector2.zero;

        foreach (Animator mAnimator in mAnimators)
        {
            mAnimator.SetBool("MOVING", isMoving);
            mAnimator.SetBool("CASTING", state == "CASTING");
            /*foreach (Skill skill in skills.skills)
                if (skill.level > 0 && !(skill.data is PassiveSkill) && AnimationHasParameter(mAnimator, skill.name))
                    mAnimator.SetBool(skill.name, skill.CastTimeRemaining() > 0);*/
            mAnimator.SetBool("STUNNED", state == "STUNNED");
            mAnimator.SetBool("DEAD", state == "DEAD");
            mAnimator.SetFloat("LookX", lookDirection.x);
            mAnimator.SetFloat("LookY", lookDirection.y);

        }

        if (cameraScript.cameraBounds != null)
        {
            Vector3 dest = clampPlayer(destination);
            if (dest != destination)
            {
                myRigidBody.transform.position = prevMove;
                hasCollided = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (hasCollided)
        {
            myRigidBody.velocity = Vector2.zero;
            hasCollided = false;
        }
        else
        {
            myRigidBody.velocity = moveDirection * speed;
            prevMove = myRigidBody.transform.position;
        }

    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("OnTriggerEnter2D - collider name:" + collider);
        if (collider.transform.parent != null)
        {
            Debug.Log("layer" + collider.transform.parent.gameObject.layer);
            if (collider.transform.parent.gameObject.layer == LayerMask.NameToLayer("Collision"))
            {
                myRigidBody.transform.position = prevMove;
                hasCollided = true;
            }
        }
    }

    protected Vector3 clampPlayer(Vector3 destination)
    {
        Vector3 offset = GetComponent<BoxCollider2D>().bounds.extents;

        return cameraScript.ClampMapWithOffset(destination, offset);
    }

    public bool IsMovementAllowed()
    {
        return true;
    }

    public void SetPlayerMap()
    {
        GameObject map = getCurrentMap(transform.localPosition);
        if (map)
            cameraScript.SetCameraBounds(map);
    }

    public GameObject getCurrentMap(Vector3 position)
    {
        GameObject[] goMaps = GameObject.FindGameObjectsWithTag("Map");
        foreach (GameObject map in goMaps)
        {
            BoxCollider2D collider = map.transform.GetChild(0).GetComponent<BoxCollider2D>();
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

    public void WarpPlayer(Vector3 position)
    {
        myRigidBody.transform.position = position;
        SetPlayerMap();
    }
}
