using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class PlayerControlled : MonoBehaviour
{
    float MaxSpeed = 2.5f;
    float Acceleration = 21.5f;
    float Drag = 5.0f;

    void Start()
    {

    }

    void Update()
    {
        var movementInput = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        velocity += (movementInput * Acceleration * Time.deltaTime);
        var movementDirection = velocity.normalized;

        // handle movement
        var speed = velocity.magnitude;
        if (speed > 0)
        {
            float drag = Drag * Time.deltaTime;
            if (speed > 0) speed = speed - Mathf.Min(speed, drag);
            if (speed > MaxSpeed) speed = MaxSpeed;

            velocity = movementDirection * speed;

            var startPos = transform.position;
            transform.position = startPos + velocity * Time.deltaTime;
        }

        // handle directional firing
        Weapon currentWeapon = GetComponent<Actor>().CurrentWeapon();
        if (currentWeapon != null && Input.GetAxis("Fire1") > 0.0f)
        {
            var mousePos = Input.mousePosition;
            mousePos.z = Camera.main.transform.position.y - transform.position.y;
            var worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            var fireDirection = (worldPos - transform.position).normalized;
            currentWeapon.Fire(fireDirection, velocity);
        }
    }

    Vector3 velocity = Vector3.zero;
}
