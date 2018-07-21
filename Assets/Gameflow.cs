
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameflow : MonoBehaviour
{
    public GameObject StartingPlayer;
    public PlayerCamera PlayerCam;
    public float PlayerRespawnTime = 3.0f;

    void Start()
    {
        RespawnPlayer();
    }

    void Update()
    {
        if (!respawningPlayer && player == null)
        {
            respawningPlayer = true;
            Invoke("RespawnPlayer", PlayerRespawnTime);
        }
    }

    void RespawnPlayer()
    {
        respawningPlayer = false;
        player = Instantiate(StartingPlayer, Vector3.zero, Quaternion.identity).GetComponent<Actor>();
        PlayerCam.Player = player;
    }

    static Actor player;
    bool respawningPlayer = false;
}
