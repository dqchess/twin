using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class PlayerControlled : MonoBehaviour
{
    public float MaxSpeed = 2.5f;
    public float BoostedMaxSpeed = 5.0f;
    public float Acceleration = 15.0f;
    public float Drag = 10.0f;

    void Start()
    {

    }

    void Update()
    {
        var dest = GetComponent<Destructible>();

        if (!dest.Destroyed)
        {
            var actor = GetComponent<Actor>();

            // handle movement
            float topSpeed = Input.GetAxis("Boost") > 0.0f ? BoostedMaxSpeed : MaxSpeed;
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

                if (speed > topSpeed) speed = topSpeed;
            }
            var movementDirection = velocity.normalized;
            velocity = movementDirection * speed;
            actor.Velocity = velocity;

            // handle directional firing
            if (Input.GetAxis("Fire1") > 0.0f)
                FireWeapon(actor.MainWeapon());
            if (Input.GetAxis("Fire2") > 0.0f)
                FireWeapon(actor.SecondaryWeapon());

            // self-destruct!
            if (Input.GetAxis("SelfDestruct") > 0.0f)
                dest.Damage(null, 999999.0f, transform.position, Vector3.forward, 2.0f);
        }
    }

    void FireWeapon(Weapon weapon)
    {
        Actor actor = GetComponent<Actor>();
        var mousePos = Input.mousePosition;
        mousePos.z = Camera.main.transform.position.y - transform.position.y;
        var worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        var fireDirection = (worldPos - transform.position).normalized;
        weapon.Fire(gameObject, fireDirection, actor.Velocity);
    }
}
