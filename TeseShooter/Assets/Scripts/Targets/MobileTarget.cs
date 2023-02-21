using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class MobileTarget : Target
{
    [SerializeField]
    List<AI_WayPoint> waypoints = new List<AI_WayPoint>();

    protected NavMeshAgent navMeshAgent;
    protected AI_WayPoint currentWayPoint = null;
    public AI_WayPoint CurrentWayPoint => currentWayPoint;

    AIMoveBehaviour moveBehaviour;

    bool activated = false;
    bool canRunAI = false;
    int StepCounter = 0;

    Rigidbody myRigidBody;
    int stepDelayCounter = 0;

    protected override void Awake()
    {
        base.Awake();
        myRigidBody = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        moveBehaviour = GetComponent<AIMoveBehaviour>();
        moveBehaviour.Initiate();
    }

    // Start is called before the first frame update
    void Start()
    {

        currentWayPoint = GetFurthestWayPoint(transform.localPosition);

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();



    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();


        if (navMeshAgent.velocity.magnitude > 0.01f) moving = true;
        else moving = false;

        if (moving && canStep)
        {
                TransmitSound();
                canStep = false;
        }

        if (!canStep)
        {
            stepDelayCounter++;
            if (stepDelayCounter > 100)
            {
                stepDelayCounter = 0;
                canStep = true;
            } 
        }

        StepCounter++;

        if (StepCounter <= 20)
        {
            
            RunAI();
            StepCounter = 0;
        }
    }


    private void RunAI()
    {
        if (moveBehaviour.Run(currentWayPoint.transform.localPosition, navMeshAgent))
        {
            moveBehaviour.Initiate();
            currentWayPoint = GetFurthestWayPoint(transform.localPosition);
        }


    }

    protected void TransmitSound()
    {
        Vector3 position = this.transform.localPosition;
        Icreature e = this;

        SoundInfo info = new SoundInfo(position, e);

        gameManager.TransmitSound(info);
    }

    public void CancelWayPoint()
    {
        currentWayPoint = null;
    }


    public AI_WayPoint GetFurthestWayPoint(Vector3 position) //get the furthest away waypoint from the position
    {
        AI_WayPoint furthest = waypoints[0];
        float furthestDist = Vector3.Distance(position, furthest.transform.localPosition);

        foreach (var w in waypoints)
        {
            float distTemp = Vector3.Distance(w.transform.localPosition, position);
            if (distTemp > furthestDist)
            {
                furthest = w;
                furthestDist = distTemp;
            }
        }

        return furthest;
    }


}
