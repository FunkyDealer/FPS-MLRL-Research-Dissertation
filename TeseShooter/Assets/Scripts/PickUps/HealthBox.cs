using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBox : MonoBehaviour
{

    [SerializeField]
    private int healthAmmount;

    [SerializeField]
    Transform StorageSpace;

    [SerializeField]
    GameManager gameManager;

    bool inPlay = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Icreature l = other.GetComponent<Icreature>();
        if (l != null)
        {
            if (inPlay && l.ReceiveHealth(healthAmmount))
            {
                inPlay = true; 
                Store();
            }
        }
    }

    public virtual void Respawn(Vector3 location, Quaternion rotation)
    {
        gameObject.SetActive(true);
        transform.position = location;
        transform.rotation = rotation;

        inPlay = true;
    }

    public void Store()
    {
        transform.position = StorageSpace.position;
        transform.rotation = Quaternion.identity;

        gameObject.SetActive(false);
    }





}
