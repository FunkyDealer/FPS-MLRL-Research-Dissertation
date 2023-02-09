using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionInfo
{
    public Vector3 pos { get; private set; }
    public float degreeRotation { get; private set; }
    public Icreature emitter { get; private set; }    


    public VisionInfo(Vector3 position, float degreeRotation,  Icreature emitter)
    {
        this.pos = position;
        this.degreeRotation = degreeRotation;
        this.emitter = emitter;
    }


}
