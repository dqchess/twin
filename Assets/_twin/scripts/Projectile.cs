using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Projectile : MonoBehaviour
{
    public float Range = 10.0f;
    public Vector3 Origin;
    public GameObject[] Visual;

    void Start()
    {
        if (Visual.Length > 0)
        {
            int visidx = Random.Range(0, Visual.Length);
            Instantiate(Visual[visidx], transform);
        }
    }

    void Update()
    {
        if (Vector3.Distance(Origin, transform.position) > Range)
            Destroy(gameObject);
    }
}
