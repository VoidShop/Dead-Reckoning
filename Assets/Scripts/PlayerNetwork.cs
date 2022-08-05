using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PlayerNetwork : NetworkBehaviour
{

    private NetworkVariable<PlayerNetworkState> playerState;
    private bool usingServerAuth;

    private Vector3 vel;
    private float rotVel;
    [SerializeField] private float cheapInterpolationTime = 0.1f;

    private void Awake()
    {
        //if the player is using server authority, let it write to the server, else write to itself
        var permission = usingServerAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        playerState = new NetworkVariable<PlayerNetworkState>(writePerm: permission);
    }

    void Update()
    {
        if (IsOwner) TransmitState();
        else ConsumeState();
    }

    private void TransmitState()
    {
        var state = new PlayerNetworkState
        {
            Position = transform.position,
            Rotation = transform.rotation.eulerAngles
        };

        if (IsServer || !usingServerAuth)
            playerState.Value = state;
        else
            TransmitStateServerRpc(state);
    }

    [ServerRpc]
    private void TransmitStateServerRpc(PlayerNetworkState state)
    {
        playerState.Value = state;
    }

    private void ConsumeState()
    {
        // Here you'll find the cheapest, dirtiest interpolation you'll ever come across. Please do better in your game (thanks tarodev)
        transform.position = Vector3.SmoothDamp(transform.position, playerState.Value.Position, ref vel, cheapInterpolationTime);

        transform.rotation = Quaternion.Euler(
            0, Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, playerState.Value.Rotation.y, ref rotVel, cheapInterpolationTime), 0);
    }

    struct PlayerNetworkState : INetworkSerializable
    {
        private float x, y, z;
        private float yRot;

        internal Vector3 Position
        {
            get => new Vector3(x, y, z);
            set
            {
                x = value.x;
                y = value.y;
                z = value.z;
            }
        }

        internal Vector3 Rotation
        {
            get => new Vector3(0, yRot, 0);
            set => yRot = value.y;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref y);
            serializer.SerializeValue(ref z);
            serializer.SerializeValue(ref yRot);

        }
    }
}