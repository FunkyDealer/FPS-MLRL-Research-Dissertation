using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviour : MonoBehaviour
{
    protected MobileTarget owner;
    public MobileTarget Owner => owner;

    protected bool arrived = false; //has the gameObject arrived yet?

    protected bool active = false; //is this behaviour active?
    public bool Active => active;

    protected virtual void Awake()
    {
        owner = GetComponent<MobileTarget>();
    }

    public virtual void Initiate()
    {

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual bool Run(GameObject target, bool inTargetRange, UnityEngine.AI.NavMeshAgent agent)
    {


        return true;
    }

    public virtual bool Run(Vector3 target, UnityEngine.AI.NavMeshAgent agent)
    {

        return true;
    }

    public void GetArrivedStatus()
    {
        if (active) arrived = true;
    }
}
