using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    public Actor Player;

    void Start()
    {

    }

    void Update()
    {
        var pos = transform.position;
        pos.x = Player.transform.position.x;
        pos.z = Player.transform.position.z;
        transform.position = pos;
    }
}
