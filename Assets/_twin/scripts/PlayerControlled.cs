using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class PlayerControlled : MonoBehaviour
{
    float MaxSpeed = 2.5f;
    float Acceleration = 15.0f;
    float Drag = 10.0f;

    void Start()
    {

    }

    void Update()
    {
        Actor actor = GetComponent<Actor>();

        // handle movement
        var movementInput = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        var velocity = actor.Velocity;
        velocity += (movementInput * Acceleration * Time.deltaTime);
        float speed = velocity.magnitude;
        if (speed > 0)
        {
            if (movementInput.magnitude == 0)
            {
                float drag = Drag * Time.deltaTime;
                speed = speed - Mathf.Min(speed, drag);
            }

            if (speed > MaxSpeed) speed = MaxSpeed;
        }
        var movementDirection = velocity.normalized;
        velocity = movementDirection * speed;
        actor.Velocity = velocity;

        // handle directional firing
        Weapon currentWeapon = actor.CurrentWeapon();
        if (currentWeapon != null && Input.GetAxis("Fire1") > 0.0f)
        {
            var mousePos = Input.mousePosition;
            mousePos.z = Camera.main.transform.position.y - transform.position.y;
            var worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            var fireDirection = (worldPos - transform.position).normalized;
            currentWeapon.Fire(fireDirection, actor.Velocity);
        }
    }
}
