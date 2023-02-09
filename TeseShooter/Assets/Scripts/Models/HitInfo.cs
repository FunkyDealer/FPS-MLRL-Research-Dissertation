using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitInfo
{
    public bool Hit { get; private set; }
    public bool Destroy { get; private set; }
    public Icreature HitEnemy { get; private set; }


    public HitInfo(bool hit, bool destroy, Icreature hitEnemy)
    {
        this.Hit = hit;
        this.Destroy = destroy;
        this.HitEnemy = hitEnemy;
    }

}
