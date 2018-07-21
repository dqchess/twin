using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Projectile : MonoBehaviour
{
    public float Range = 10.0f;
    public Vector3 Origin;
    public GameObject OriginObject;   // actor may have moved so store origin separately
    public Vector2 Damage = new Vector2(1.0f, 1.0f);
    public float HomingAmount = 0.0f;
    public float HomingAngle = 90.0f;
    public bool DestroyOnImpact = true;
    public float ImpactStrength = 1.0f;
    public GameObject[] Visual;
    public TempEffect[] ImpactEffect;
    public TempEffect[] FizzleEffect;

    void Start()
    {
        if (Visual.Length > 0 )
            Instantiate(RandomUtils.Pick(Visual), transform);
    }

    void Update()
    {
        if (Vector3.Distance(Origin, transform.position) > Range)
        {
            if (FizzleEffect.Length > 0)
                Instantiate(RandomUtils.Pick(FizzleEffect), transform.position, transform.rotation);
            Destroy(gameObject);
        }

        homingTarget = PickHomingTarget();
        if (homingTarget)
            UpdateHoming();
    }

    void OnTriggerEnter(Collider collision)
    {
        Destructible dest = collision.attachedRigidbody.GetComponent<Destructible>();
        if (dest != null && WouldDamage(dest.gameObject))
        {
            float damage = Random.Range(Damage.x, Damage.y);
            dest.Damage(damage, transform.position, GetComponent<Actor>().Velocity.normalized, ImpactStrength);

            if (DestroyOnImpact)
            {
                if (ImpactEffect.Length > 0)
                    Instantiate(RandomUtils.Pick(ImpactEffect), transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
    }

    Destructible PickHomingTarget()
    {
        if (HomingAmount > 0.0f)
        {
            var nearby = Physics.OverlapSphere(transform.position, Range);
            if (nearby.Length > 0)
            {
                foreach (var c in nearby)
                {
                    var destructible = c.attachedRigidbody.GetComponent<Destructible>();
                    float halfHomingAngleRad = Mathf.Deg2Rad * HomingAngle * 0.5f;
                    if (destructible != null && WouldDamage(destructible.gameObject) && WithinAngle(destructible.gameObject, halfHomingAngleRad))
                        return destructible;
                }
            }
        }
        return null;
    }

    void UpdateHoming()
    {
        var vecToTarget = homingTarget.transform.position - transform.position;
        var dirToTarget = vecToTarget.normalized;

        var velocity = GetComponent<Actor>().Velocity;
        float speed = velocity.magnitude;

        velocity = speed * Vector3.RotateTowards(velocity.normalized, dirToTarget, HomingAmount * Time.deltaTime, 0.0f);

        GetComponent<Actor>().Velocity = velocity;
    }

    bool WouldDamage(GameObject obj)
    {
        return OriginObject != obj;
    }

    bool WithinAngle(GameObject obj, float Angle)
    {
        var vecToTarget = obj.transform.position - transform.position;
        var dirToTarget = vecToTarget.normalized;
        var dir = GetComponent<Actor>().Velocity.normalized;
        float angle = Vector3.Angle(dirToTarget, dir);
        return Mathf.Deg2Rad * angle < Angle;
    }

    Destructible homingTarget;
}
