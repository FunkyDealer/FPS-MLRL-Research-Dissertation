using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaProjectile : Projectile
{
    [SerializeField]
    float velocity;

    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(CountDownToDeath(lifeTime));

    }

    // Update is called once per frame
    void Update()
    {


    }
    private void FixedUpdate()
    {
        transform.localPosition += direction * velocity * 0.04f;
    }

    private IEnumerator CountDownToDeath(float time)
    {
        yield return new WaitForSeconds(time);

        //this.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isShooter = false;

        Icreature l = other.GetComponent<Icreature>();
        if (l != null)
        {
            if (l == shooter) isShooter = true;
            l.ReceiveDamage(damage);
        }


       if (!isShooter) Destroy(gameObject);
    }
}
