using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Icreature
{
    public bool ReceiveHealth(int ammount);
    public void SetToSleep();
    public bool ReceiveDamage(int ammount);
    public void ReceiveLocation(VisionInfo info);
    public void receiveSound(SoundInfo sound);
    public void Respawn(Vector3 location, Quaternion rotation);
    public void SetGameManager(GameManager manager);
    public void SetHitStatus(HitInfo info);
    public GameObject GetGameObject();
    public void Store();



}
