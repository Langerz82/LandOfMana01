using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;

//using Rigidbody2D = UnityEngine.Physics2DModule.Rigidbody2D;
using SpriteLibrary = UnityEngine.U2D.Animation.SpriteLibrary;
using SpriteLibraryAsset = UnityEngine.U2D.Animation.SpriteLibraryAsset;

//using CameraMMO2D;

using Debug = UnityEngine.Debug;

public class Player : MonoBehaviour
{
    [HideInInspector] public Vector2 lookDirection;
    public float speed = 1/16f;

    [HideInInspector] public CameraMMO2D cameraScript;
    [HideInInspector] public GameObject playerCamera;
    [HideInInspector] public Rigidbody2D myRigidBody;

    [HideInInspector] public string state;

    protected Animator[] mAnimators = new Animator[2];
    protected SpriteLibrary[] mySpriteLibs = new SpriteLibrary[2];

    // Start is called before the first frame update
    void Start()
    {
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
// END MOD
    }

    // Update is called once per frame
    void Update()
    {
        lookDirection = Vector2.zero;

        Vector3 destination = myRigidBody.transform.position;

        if (Input.GetKey(KeyCode.W))
        {
            lookDirection.y = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            lookDirection.y = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            lookDirection.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            lookDirection.x = 1;
        }

        if (lookDirection.x != 0)
        {
            destination.x += (lookDirection.x * speed);
        }
        if (lookDirection.y != 0)
        {
            destination.y += (lookDirection.y * speed);
        }

// BEGIN MOD - JL - 5/6/24 - 12/6/24
        //bool isMoving = movement.IsMoving() && state != "CASTING" && !mountControl.IsMounted();
        bool isMoving = lookDirection != Vector2.zero;

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

        destination = clampPlayer(destination);

        bool hit = Physics2D.Raycast(destination, -Vector2.up, 1, 1 << LayerMask.NameToLayer("Collision"));

        if (!hit)
            myRigidBody.transform.position = destination;
    }

    protected Vector3 clampPlayer(Vector3 destination)
    {
        Vector3 offset = GetComponent<CircleCollider2D>().bounds.extents;
        //offset.x /= 2;
        //offset.y /= 2;

        return cameraScript.ClampMapWithOffset(destination, offset);
    }

    public bool IsMovementAllowed()
    {
        return true;
    }
}
