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

        GameObject[] goMaps = GameObject.FindGameObjectsWithTag("Map");
        foreach(GameObject map in goMaps)
        {
            if (map.GetComponent<BoxCollider2D>().bounds.Contains(transform.localPosition))
                SetPlayerMap(map);
        }
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = Vector2.zero;

        Vector3 destination = myRigidBody.transform.position;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection.y = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection.y = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection.x = 1;
        }
        if (moveDirection != Vector2.zero)
        {
            lookDirection = moveDirection;
        }

        if (lookDirection.x != 0)
        {
            destination.x += (moveDirection.x * speed);
        }
        else if (lookDirection.y != 0)
        {
            destination.y += (moveDirection.y * speed);
        }

        // BEGIN MOD - JL - 5/6/24 - 12/6/24
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
        // END MOD

        if (cameraScript.cameraBounds != null)
            destination = clampPlayer(destination);

        //  README - The problem with this collision code is it does not respect the colliders dimensions.
        //      So the collision will only take effect in the middle of the gamobject and not it's collision sides.

        //bool hit = Physics2D.Raycast(destination, -Vector2.up, 1, 1 << LayerMask.NameToLayer("Collision"));
        //if (!hit)
            myRigidBody.transform.position = destination;
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("OnTriggerEnter - collider name:" + collider);
    }

    void OnCollisionEnter (Collision collision)
    {
        Debug.Log("OnCollisionEnter - collision name:" + collision);
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

    public void SetPlayerMap(GameObject goMap)
    {
        cameraScript.SetCameraBounds(goMap);
    }

}

