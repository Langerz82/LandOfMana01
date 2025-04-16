using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class Main : MonoBehaviour
{
    public GameObject[] maps;
    public GameObject mainPlayer;

    [HideInInspector] public Player playerScript;

    // Start is called before the first frame update
    void Start()
    {
        playerScript = mainPlayer.GetComponent<Player>();
        maps = GameObject.FindGameObjectsWithTag("Map");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
