using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;

using SpriteLibrary = UnityEngine.U2D.Animation.SpriteLibrary;
using SpriteLibraryAsset = UnityEngine.U2D.Animation.SpriteLibraryAsset;

using Debug = UnityEngine.Debug;
using Math = System.Math;
using System.Linq;

[RequireComponent(typeof(PlayerMovement))]
public class Player : Entity
{
    [Header("Components")]
    protected EntityMovement myMovement;
    protected EntityAttack myEntityAttack;

    [HideInInspector] public string state;

    protected Animator[] myAnimators = new Animator[2];
    public SpriteLibrary[] mySpriteLibs = new SpriteLibrary[2];

    //[HideInInspector] public bool isAttacking;

    // Start is called before the first frame update
    void Start()
    {
        myMovement = GetComponent<EntityMovement>();
        myEntityAttack = GetComponent<EntityAttack>();

        Transform childTF = transform.GetChild(0).Find("sprites").Find("sprite_body");
        if (childTF)
        {
            Debug.Log("childTF - found");
            myAnimators[0] = GetComponent<Animator>();
            if (!myAnimators[0]) myAnimators[0] = childTF.GetComponent<Animator>();
            if (!myAnimators[0]) Debug.Log("Player - mAnimators[0] not found.");
            mySpriteLibs[0] = childTF.GetComponent<SpriteLibrary>();
            if (!mySpriteLibs[0]) Debug.Log("BODY SPRITELIB NOT FOUND.");
        }

        childTF = transform.GetChild(0).Find("sprites").Find("sprite_weapon");
        if (childTF)
        {
            Debug.Log("childTF2 - found");
            myAnimators[1] = GetComponent<Animator>();
            if (!myAnimators[1]) myAnimators[1] = childTF.GetComponent<Animator>();
            if (!myAnimators[1]) Debug.Log("Player - mAnimators[1] not found.");
            mySpriteLibs[1] = childTF.GetComponent<SpriteLibrary>();
            if (!mySpriteLibs[1]) Debug.Log("WEAPON SPRITELIB NOT FOUND.");
        }
        EventDeath += OnDeath;
        EventRespawn += OnRespawn;
    }

    // Update is called once per frame
    void Update()
    {
        bool isMoving = (myMovement.myRigidbody.velocity != Vector2.zero);

        foreach (Animator mAnimator in myAnimators)
        {
            mAnimator.SetBool("MOVING", isMoving);
            mAnimator.SetBool("DEAD", state == "DEAD");
            mAnimator.SetFloat("LookX", myMovement.lookDirection.x);
            mAnimator.SetFloat("LookY", myMovement.lookDirection.y);
            mAnimator.SetBool("Attack", GetComponent<EntityAttack>().target != null);
        }
    }

    public void OnDeath(GameObject killer)
    {
        Destroy(this.transform.gameObject);
    }

    public void OnRespawn()
    {
        state = "IDLE";
    }
}
