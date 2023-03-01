using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIWanderBehaviour : AIBehaviour
{
    [SerializeField]
    private float WanderMaxDistance = 10;

    protected override void Awake()
    {
        base.Awake();

    }

    public override void Initiate()
    {
        base.Initiate();
        active = true;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ResetMove()
    {
        arrived = false;
        active = false;
        owner.CancelWayPoint();
    }

    public override bool Run(Vector3 target, NavMeshAgent agent)
    {
        Vector3 destination = Vector3.zero;

        NavMeshHit navHit;
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomDirection = transform.localPosition + UnityEngine.Random.insideUnitSphere * WanderMaxDistance;
            randomDirection.y = Random.Range(-1, 1);

            if (NavMesh.SamplePosition(randomDirection, out navHit, WanderMaxDistance, NavMesh.AllAreas))
            {
                if (Vector3.Distance(navHit.position, transform.position) <= WanderMaxDistance)
                {
                    destination = navHit.position;
                    break;
                }
            }
        }


        if (agent.enabled) agent.SetDestination(destination);


        return true;
    }


    public void ForceEnd()
    {
        ResetMove();
    }
}
