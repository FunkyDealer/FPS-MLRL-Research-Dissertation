using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPRadar : MonoBehaviour
{
    BattlePlayer player;


    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<BattlePlayer>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {



    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {

            player.ObstacleAvoidancePenalty();
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }
}
