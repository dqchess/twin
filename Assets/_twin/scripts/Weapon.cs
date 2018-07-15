
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Projectile Projectile;
    public float ProjectileSpeed = 1.0f;
    public float ProjectileVelocityInheritFactor = 1.0f;
    public float CooldownTime = 1.0f;
    public float SpawnAheadDistance = 0.0f;

    void Start()
    {
        cooldownRemaining = CooldownTime;
    }

    void Update()
    {
        if (cooldownRemaining > 0.0f)
            cooldownRemaining -= Time.deltaTime;
    }

    public void Fire(Vector3 direction, Vector3 originVelocity)
    {
        Debug.Assert(Projectile != null, "weapon has no projectile set");

        if (cooldownRemaining <= 0.0f)
        {
            cooldownRemaining += CooldownTime;
            var proj = GameObject.Instantiate(Projectile.gameObject, transform.position + direction * SpawnAheadDistance, Quaternion.LookRotation(direction, Vector3.up));
            proj.GetComponent<Projectile>().Origin = transform.position;
            proj.GetComponent<Actor>().Velocity = direction * ProjectileSpeed + originVelocity * ProjectileVelocityInheritFactor;
        }
    }

    float cooldownRemaining;
}
