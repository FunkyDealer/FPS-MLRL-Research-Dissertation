using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : Projectile
{
    LineRenderer lineRenderer;

    Vector3 start = Vector3.zero;
    Vector3 end;

    [SerializeField]
    private LayerMask HittableLayers;

    HitInfo hitInfo;

    Color HitEnemyColor = Color.red;
    Color HitEnvironmentColor = Color.yellow;
    Color HitNothingColor = Color.blue;


    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.localPosition, direction, out hit, 100, HittableLayers))
        {
            end = hit.point;
            end = start + direction * hit.distance;

            Icreature l = hit.collider.GetComponent<Icreature>();
            if (l != null)
            {
                lineRenderer.startColor = HitEnemyColor;
                lineRenderer.endColor = HitEnemyColor;

                bool killed = l.ReceiveDamage(damage);
                shooter.SetHitStatus(new HitInfo( true, killed, l));
            }
            else
            {
                lineRenderer.startColor = HitEnvironmentColor;
                lineRenderer.endColor = HitEnvironmentColor;

                shooter.SetHitStatus(new HitInfo(false, false, null));
            }
        }
        else
        {
            end = start + direction * 100;

            lineRenderer.startColor = HitNothingColor;
            lineRenderer.endColor = HitNothingColor;
            shooter.SetHitStatus(new HitInfo(false, false, null));
        }

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        StartCoroutine(CountDownToDeath(lifeTime));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator CountDownToDeath(float time)
    {
        yield return new WaitForSeconds(time);

        //this.gameObject.SetActive(false);
        Destroy(gameObject);
    }



}
