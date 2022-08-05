using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using System;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private JoinPanel joinPanel;
    [SerializeField] private CreatePanel createPanel;
    [SerializeField] private RoomPanel roomPanel;

    void Start()
    {
        joinPanel.gameObject.SetActive(true);
        createPanel.gameObject.SetActive(false);
        roomPanel.gameObject.SetActive(false);

        CreatePanel.LobbyCreated += CreateLobby;
        LobbyRoomPanel.LobbySelected += OnLobbySelected;
        RoomPanel.LobbyLeft += OnLobbyLeft;
        RoomPanel.StartPressed += OnGameStart;

        NetworkObject.DestroyWithScene = true;
    }

    private async void OnLobbySelected(Lobby lobby)
    {
        //using (new Load("Joining Lobby..."))
        //{
            try
            {
                await MatchmakingService.JoinLobbyWithAllocation(lobby.Id);

                joinPanel.gameObject.SetActive(false);
                roomPanel.gameObject.SetActive(true);

                NetworkManager.Singleton.StartClient();
            }
        catch (Exception e)
        {
            Debug.LogError(e);
            //CanvasUtilities.Instance.ShowError("Failed joining lobby");
        }
        //}
    }

    private async void CreateLobby(LobbyData data)
    {
        //using (new Load("Creating Lobby..."))
        //{
        try
        {
            await MatchmakingService.CreateLobbyWithAllocation(data);

            createPanel.gameObject.SetActive(false);
            roomPanel.gameObject.SetActive(true);

            // Starting the host immediately will keep the relay server alive
            NetworkManager.Singleton.StartHost();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            //CanvasUtilities.Instance.ShowError("Failed creating lobby");
        }
    //}
    }

    private readonly Dictionary<ulong, bool> playersInLobby = new();
    public static event Action<Dictionary<ulong, bool>> LobbyPlayersUpdated;
    private float nextLobbyUpdate;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            playersInLobby.Add(NetworkManager.Singleton.LocalClientId, false);
            UpdateInterface();
        }

        // Client uses this in case host destroys the lobby
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;


    }

    private void OnClientConnectedCallback(ulong playerId)
    {
        if (!IsServer) return;

        // Add locally
        if (!playersInLobby.ContainsKey(playerId)) playersInLobby.Add(playerId, false);

        PropagateToClients();

        UpdateInterface();
    }

    private void PropagateToClients()
    {
        foreach (var player in playersInLobby) UpdatePlayerClientRpc(player.Key, player.Value);
    }

    [ClientRpc]
    private void UpdatePlayerClientRpc(ulong clientId, bool isReady)
    {
        if (IsServer) return;

        if (!playersInLobby.ContainsKey(clientId)) playersInLobby.Add(clientId, isReady);
        else playersInLobby[clientId] = isReady;
        UpdateInterface();
    }

    private void OnClientDisconnectCallback(ulong playerId)
    {
        if (IsServer)
        {
            // Handle locally
            if (playersInLobby.ContainsKey(playerId)) playersInLobby.Remove(playerId);

            // Propagate all clients
            RemovePlayerClientRpc(playerId);

            UpdateInterface();
        }
        else
        {
            // This happens when the host disconnects the lobby
            roomPanel.gameObject.SetActive(false);
            joinPanel.gameObject.SetActive(true);
            OnLobbyLeft();
        }
    }

    [ClientRpc]
    private void RemovePlayerClientRpc(ulong clientId)
    {
        if (IsServer) return;

        if (playersInLobby.ContainsKey(clientId)) playersInLobby.Remove(clientId);
        UpdateInterface();
    }

    public void OnReadyClicked()
    {
        SetReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ulong playerId)
    {
        playersInLobby[playerId] = true;
        PropagateToClients();
        UpdateInterface();
    }

    private void UpdateInterface()
    {
        LobbyPlayersUpdated?.Invoke(playersInLobby);
    }

    private async void OnLobbyLeft()
    {
        //using (new Load("Leaving Lobby..."))
        //{
            playersInLobby.Clear();
            NetworkManager.Singleton.Shutdown();
            await MatchmakingService.LeaveLobby();
        //}
    }

    public override void OnDestroy()
    {

        base.OnDestroy();
        CreatePanel.LobbyCreated -= CreateLobby;
        LobbyRoomPanel.LobbySelected -= OnLobbySelected;
        RoomPanel.LobbyLeft -= OnLobbyLeft;
        RoomPanel.StartPressed -= OnGameStart;

        // We only care about this during lobby
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

    }

    private async void OnGameStart()
    {
        //using (new Load("Starting the game..."))
        //{
            await MatchmakingService.LockLobby();
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        //}
    }
}
