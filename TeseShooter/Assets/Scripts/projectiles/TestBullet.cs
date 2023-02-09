using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBullet : Projectile
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
        transform.position += direction * velocity * 0.04f;
    }

    private IEnumerator CountDownToDeath(float time)
    {
        yield return new WaitForSeconds(time);

        //this.gameObject.SetActive(false);
        Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        Icreature l = other.GetComponent<Icreature>();
        if (l != null)
        {
            l.ReceiveDamage(damage);
        }


        Destroy(gameObject);
    }



}
