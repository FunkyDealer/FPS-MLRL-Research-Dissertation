using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    protected float lifeTime;

    [HideInInspector]
    public Vector3 direction;

    [SerializeField]
    protected int damage;

    [HideInInspector]
    public Icreature shooter;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
