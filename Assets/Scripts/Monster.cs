using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using SpriteLibrary = UnityEngine.U2D.Animation.SpriteLibrary;
using SpriteLibraryAsset = UnityEngine.U2D.Animation.SpriteLibraryAsset;

using Debug = UnityEngine.Debug;
//using Math = System.Math;
//using System.Linq;

[RequireComponent(typeof(MonsterMovement))]
public class Monster : Entity
{
    protected MonsterMovement myMovement;
    protected EntityDrop myEntityDrop;
    protected EntityAttack myEntityAttack;

    [HideInInspector] public string state;

    protected Animator myAnimator;
    protected SpriteLibrary mySpriteLib;

    protected Main mainScript;

    // Start is called before the first frame update
    void Start()
    {
        myMovement = GetComponent<MonsterMovement>();
        myEntityDrop = GetComponent<EntityDrop>();
        myEntityAttack = GetComponent<EntityAttack>();

        mainScript = GameObject.FindWithTag("Main").GetComponent<Main>();

        Transform childTF = transform.Find("sprites").Find("sprite_body");
        if (childTF)
        {
            Debug.Log("childTF - found");
            myAnimator = GetComponent<Animator>();
            if (!myAnimator) myAnimator = childTF.GetComponent<Animator>();
            if (!myAnimator) Debug.Log("Player - mAnimators[0] not found.");
            mySpriteLib = childTF.GetComponent<SpriteLibrary>();
            if (!mySpriteLib) Debug.Log("BODY SPRITELIB NOT FOUND.");
        }
        EventDeath += Death;
    }

    // Update is called once per frame
    void Update()
    {
        bool isMoving = (myMovement.myRigidbody.velocity != Vector2.zero);

        myAnimator.SetBool("MOVING", isMoving);
        myAnimator.SetBool("DEAD", state == "DEAD");
        myAnimator.SetFloat("LookX", myMovement.lookDirection.x);
        myAnimator.SetFloat("LookY", myMovement.lookDirection.y);
        myAnimator.SetBool("Attack", myEntityAttack.target != null);
    }

    public void Death()
    {
        //myEntityDrop.CreateDrop();
        Destroy(this.transform.gameObject);
    }

}
