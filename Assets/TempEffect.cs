using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TempEffect : MonoBehaviour
{
    public AudioClip SoundEffect;
    public float Lifespan = 5.0f;

    void Start()
    {
        GetComponent<AudioSource>().PlayOneShot(SoundEffect);
        Destroy(gameObject, Lifespan);
    }

    void Update()
    {
    }
}
