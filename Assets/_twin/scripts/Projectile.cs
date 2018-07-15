using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(AudioSource))]
public class Projectile : MonoBehaviour
{
    public float Range = 10.0f;
    public Vector3 Origin;
    public Vector2 Damage = new Vector2(1.0f, 1.0f);
    public GameObject[] Visual;
    public bool DestroyOnImpact = true;
    public AudioClip ImpactSound;

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

    void OnTriggerEnter(Collider collision)
    {
        Destructible dest = collision.attachedRigidbody.GetComponent<Destructible>();
        if (dest != null)
        {
            float damage = Random.Range(Damage.x, Damage.y);
            dest.Damage(damage);

            GetComponent<AudioSource>().PlayOneShot(ImpactSound);

            if (DestroyOnImpact)
            {
                GetComponent<Actor>().Hide();
                Invoke("Explode", 2);
            }
        }
    }

    void Explode()
    {
        Destroy(gameObject);
    }
}
