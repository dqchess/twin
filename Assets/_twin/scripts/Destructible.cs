using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Actor))]
public class Destructible : MonoBehaviour
{
    public Vector2 HitPointRange = new Vector2(10.0f, 10.0f);
    public float HitPoints
    {
        get;
        private set;
    }
    public TempEffect DestroyEffect;

    public void Damage(float Amount)
    {
        HitPoints -= Amount;
        if (HitPoints <= 0.0f)
        {
            Instantiate(DestroyEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HitPoints = Random.Range(HitPointRange.x, HitPointRange.y);
    }

    void Update()
    {

    }
}
