using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(Destructible))]
public class NPC : MonoBehaviour
{
    public Actor CurrentTarget;

    void Start()
    {

    }

    void Update()
    {
        Destructible d = GetComponent<Destructible>();

        if (!d.Destroyed)
        {
            if (CurrentTarget != null)
            {
                Actor a = GetComponent<Actor>();
                Weapon w = a.MainWeapon();

                Vector3 vecToTarget = CurrentTarget.transform.position - transform.position;
                float distToTarget = vecToTarget.magnitude;

                if (distToTarget < w.Projectile.Range)
                {
                    Vector3 dirToTarget = vecToTarget.normalized;
                    transform.rotation = Quaternion.LookRotation(dirToTarget, Vector3.up);
                    w.Fire(gameObject, dirToTarget, a.Velocity);
                }
            }
        }
    }
}
