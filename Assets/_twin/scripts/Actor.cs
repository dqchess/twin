using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public Vector3 Velocity { get; set; }
    public float Speed { get { return Velocity.magnitude; } }
    public Weapon[] Weapons = new Weapon[0];

    public Weapon CurrentWeapon()
    {
        if (currentWeapon != null)
            return currentWeapon;
        if (Weapons.Length > 0)
            return Weapons[0];
        return null;
    }

    void Update()
    {
        var movementDirection = Velocity.normalized;

        // handle movement
        transform.position = transform.position + Velocity * Time.deltaTime;
    }

    Weapon currentWeapon;
}

