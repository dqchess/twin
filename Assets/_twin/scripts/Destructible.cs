using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Destructible : MonoBehaviour
{
    public Vector2 HitPointRange = new Vector2(10.0f, 10.0f);
    public float HitPoints
    {
        get;
        private set;
    }

    public void Damage(float Amount)
    {
        HitPoints -= Amount;
        if (HitPoints <= 0.0f)
            Explode();
    }

    public void Explode()
    {
        Destroy(gameObject);
    }

    void Start()
    {
        HitPoints = Random.Range(HitPointRange.x, HitPointRange.y);
    }

    void Update()
    {

    }
}
