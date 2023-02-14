using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : Spawner
{



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SpawnEntity(Icreature obj)
    {
        obj.Respawn(this.transform.position, this.transform.rotation);



    }
}
