using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Actor))]
public class Destructible : MonoBehaviour
{
    public Vector2 HitPointRange = new Vector2(10.0f, 10.0f);
    public float HitPoints
    {
        get;
        private set;
    }
    public AudioClip ExplodeSound;

    public void Damage(float Amount)
    {
        HitPoints -= Amount;
        if (HitPoints <= 0.0f)
        {
            GetComponent<Actor>().Hide();
            GetComponent<AudioSource>().PlayOneShot(ExplodeSound);
            Invoke("Explode", 5);
        }
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
