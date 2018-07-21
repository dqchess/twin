using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public Vector3 Velocity { get; set; }
    public float Speed { get { return Velocity.magnitude; } }
    public Weapon[] Weapons = new Weapon[0];
    public bool RotateToMovementDirection = true;
    public float RotationRate = 0.15f;
    public float LargeDirectionChangeMaxSpeed = 1.0f;

    public Weapon MainWeapon()
    {
        if (Weapons.Length > 0)
            return Weapons[0];
        return null;
    }

    public Weapon SecondaryWeapon()
    {
        if (Weapons.Length > 1)
            return Weapons[1];
        return null;
    }

    void Update()
    {
        // update position
        transform.position = transform.position + Velocity * Time.deltaTime;

        // update orientation
        if (RotateToMovementDirection && Speed > 0)
        {
            var movementDirection = Velocity.normalized;
            var targetOrientation = movementDirection;
            var orientation = transform.rotation * Vector3.forward;
            var newOrientation = Vector3.RotateTowards(orientation, targetOrientation, RotationRate, 0.0f);
            transform.rotation = Quaternion.LookRotation(newOrientation, Vector3.up);

            // if we're turning by a large amount, clamp velocity until we're closer to the target angle
            // (to keep the actor from looking like it's moving backwards)
            if (Vector3.Angle(targetOrientation, newOrientation) > Mathf.PI * 0.75f)
                Velocity = Velocity.normalized * Mathf.Min(LargeDirectionChangeMaxSpeed, Speed);
        }
    }
}

