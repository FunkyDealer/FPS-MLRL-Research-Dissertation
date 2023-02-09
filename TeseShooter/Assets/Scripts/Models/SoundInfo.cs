using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundInfo 
{
    public Vector3 pos { get; private set; }
    public Icreature emitter { get; private set; }


    public SoundInfo(Vector3 position, Icreature emitter)
    {
        this.pos = position;
        this.emitter = emitter;
    }

}
