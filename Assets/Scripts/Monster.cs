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

    public float m_DeathTimeLength = 30f;
    protected float m_DeathTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        myMovement = GetComponent<MonsterMovement>();
        myEntityDrop = GetComponent<EntityDrop>();
        myEntityAttack = GetComponent<EntityAttack>();

        EventDeath += OnDeath;
        EventRespawn += OnRespawn;

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

        OnRespawn();
    }

    // Update is called once per frame
    void Update()
    {
        m_DeathTimer += Time.deltaTime;
        if (m_DeathTimer > m_DeathTimeLength)
        {
            Respawn();
            m_DeathTimer = 0;
        }

        if (state == "DEAD")
            return;

        bool isMoving = (myMovement.myRigidbody.velocity != Vector2.zero);

        myAnimator.SetBool("MOVING", isMoving);
        myAnimator.SetBool("DEAD", state == "DEAD");
        myAnimator.SetFloat("LookX", myMovement.lookDirection.x);
        myAnimator.SetFloat("LookY", myMovement.lookDirection.y);
        myAnimator.SetBool("Attack", myEntityAttack.target != null);
    }

    // TODO
    protected void OnDeath()
    {
        //myEntityDrop.CreateDrop();
        //Destroy(this.transform.gameObject);
        //this.transform.position.z = -1;
        state = "DEAD";
    }

    protected void OnRespawn()
    {
        //this.transform.position.z = 0;
        state = "IDLE";
    }
}
