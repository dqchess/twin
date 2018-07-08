
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float CooldownTime = 1.0f;

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
        if (cooldownRemaining <= 0.0f)
        {
            Debug.Log("Firing weapon, direction = " + direction);
            cooldownRemaining += CooldownTime;
        }
    }

    float cooldownRemaining;
}
