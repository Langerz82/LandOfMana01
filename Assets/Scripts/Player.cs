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
public class Player : MonoBehaviour
{
    [Header("Components")]
    public PlayerMovement playerMovement;

    [HideInInspector] public string state;

    protected Animator[] mAnimators = new Animator[2];
    protected SpriteLibrary[] mySpriteLibs = new SpriteLibrary[2];

    // Start is called before the first frame update
    void Start()
    {
        //goMain = GameObject.FindWithTag("Main");
        //mainScript = goMain.GetComponent<mainScript>();

        // BEGIN MOD - JL - 5/6/24 - 12/6/24
        //mAnimators = new Animator[2];
        //mySpriteLibs = new SpriteLibrary[2];

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
    }

    // Update is called once per frame
    void Update()
    {

        //bool isMoving = movement.IsMoving() && state != "CASTING" && !mountControl.IsMounted();
        bool isMoving = playerMovement.myRigidbody.velocity != Vector2.zero;

        foreach (Animator mAnimator in mAnimators)
        {
            mAnimator.SetBool("MOVING", isMoving);
            mAnimator.SetBool("CASTING", state == "CASTING");
            /*foreach (Skill skill in skills.skills)
                if (skill.level > 0 && !(skill.data is PassiveSkill) && AnimationHasParameter(mAnimator, skill.name))
                    mAnimator.SetBool(skill.name, skill.CastTimeRemaining() > 0);*/
            mAnimator.SetBool("STUNNED", state == "STUNNED");
            mAnimator.SetBool("DEAD", state == "DEAD");
            mAnimator.SetFloat("LookX", playerMovement.lookDirection.x);
            mAnimator.SetFloat("LookY", playerMovement.lookDirection.y);

        }

    }

}
