using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class WanderingTarget : Target
{

    protected NavMeshAgent navMeshAgent;

    AIWanderBehaviour wanderBehaviour;

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

        wanderBehaviour = GetComponent<AIWanderBehaviour>();
        wanderBehaviour.Initiate();
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
        if (wanderBehaviour.Run(navMeshAgent.destination, navMeshAgent))
        {
            wanderBehaviour.Initiate();
        }


    }

    protected void TransmitSound()
    {
        Vector3 position = this.transform.localPosition;
        Icreature e = this;

        SoundInfo info = new SoundInfo(position, e);

        gameManager.TransmitSound(info);
    }

}
