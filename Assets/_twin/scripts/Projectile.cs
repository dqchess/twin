using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Projectile : MonoBehaviour
{
    public float Range = 10.0f;
    public Vector3 Origin;
    public Vector2 Damage = new Vector2(1.0f, 1.0f);
    public GameObject[] Visual;
    public bool DestroyOnImpact = true;
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
    }

    void OnTriggerEnter(Collider collision)
    {
        Destructible dest = collision.attachedRigidbody.GetComponent<Destructible>();
        if (dest != null)
        {
            float damage = Random.Range(Damage.x, Damage.y);
            dest.Damage(damage, transform.position, GetComponent<Actor>().Velocity.normalized);

            if (DestroyOnImpact)
            {
                if (ImpactEffect.Length > 0)
                    Instantiate(RandomUtils.Pick(ImpactEffect), transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
    }
}
