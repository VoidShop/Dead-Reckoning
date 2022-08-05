using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Abilities : NetworkBehaviour
{
    public GameObject fireBall;

    private void Update()
    {
        //makes sure we own player
        if (!IsOwner) return;

        //on key press (Q)
        if (Input.GetKeyDown(KeyCode.Q) && !Input.GetKey(KeyCode.LeftShift))
        {
            //Send a message to the server to execute this method
            RequestProjectileServerRpc();

            //spawn fireball instantly for ourself (no need to wait for server to handle this)
            ShootProjectile();
        }
    }


    [ServerRpc]
    private void RequestProjectileServerRpc()
    {
        //sends a message to all clients to execute this method
        FireProjectileClientRpc();
    }

    [ClientRpc]
    private void FireProjectileClientRpc()
    {
        //makes sure we dont spawn two fireballs
        if (!IsOwner) { ShootProjectile(); }
    }


    void ShootProjectile()
    {
        //spawns fireball
        Instantiate(fireBall, transform.position + transform.forward, transform.rotation);
    }
}