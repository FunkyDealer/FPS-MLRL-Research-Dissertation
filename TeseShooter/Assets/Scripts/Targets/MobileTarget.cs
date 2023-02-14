using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class MobileTarget : Target
{
    protected NavMeshAgent navMeshAgent;
    protected AI_WayPoint currentWayPoint = null;
    public AI_WayPoint CurrentWayPoint => currentWayPoint;


    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();



    }
}
