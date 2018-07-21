using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    public Actor Player;
    public float Distance = 20.0f;

    void Start()
    {

    }

    void Update()
    {
        if (Player != null)
        {
            var pos = transform.position;
            pos.x = Player.transform.position.x;
            pos.y = Distance;
            pos.z = Player.transform.position.z;
            transform.position = pos;
        }
    }
}
