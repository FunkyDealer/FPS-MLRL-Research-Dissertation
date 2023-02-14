using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_WayPoint : MonoBehaviour
{
    List<AIBehaviour> AIinside;

    // Start is called before the first frame update
    void Start()
    {
        AIinside = new List<AIBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {



    }


    private void FixedUpdate()
    {
        if (AIinside.Count > 0)
        {
            foreach (var a in AIinside)
            {
                if (a.Owner.CurrentWayPoint == this)
                {
                    a.GetArrivedStatus();
                }
            }
        }

    }

    private void OnTriggerStay(Collider other)
    {


    }


    private void OnTriggerEnter(Collider other)
    {
        AIBehaviour temp = other.GetComponent(typeof(AIBehaviour)) as AIBehaviour;
        if (temp != null)
        {
            AIinside.Add(temp);
        }


        if (temp != null && temp.Owner.CurrentWayPoint == this)
        {
            temp.GetArrivedStatus();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        AIBehaviour temp = other.GetComponent(typeof(AIBehaviour)) as AIBehaviour;
        if (temp != null)
        {
            AIinside.Remove(temp);
        }
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Handles.color = Color.red;
    //    Handles.Label(transform.position, gameObject.name);
    //}
}
