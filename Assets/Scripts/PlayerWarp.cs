using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class PlayerWarp : MonoBehaviour
{
    public GameObject warpDestination;
    [HideInInspector] public bool hasWarped = false;

    protected BoxCollider2D myCollider;

    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("OnTriggerEnter2D");
        if (hasWarped)
            return;

        hasWarped = true;

        if (collider.gameObject.tag == "Player")
            WarpPlayer(collider.gameObject);
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        Debug.Log("OnTriggerExit2D");
        hasWarped = false;
    }

    void WarpPlayer(GameObject player)
    {
        Debug.Log("WarpPlayer");
        Rigidbody2D rigidBody = player.GetComponent<Rigidbody2D>();
        Vector3 newPos = rigidBody.transform.position - myCollider.gameObject.transform.position;
        player.GetComponent<Player>().WarpPlayer(warpDestination.transform.position + newPos);
        warpDestination.GetComponent<PlayerWarp>().hasWarped = true;
    }
}
