using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     NetworkBehaviours cannot easily be parented, so the network logic will take place
///     on the network scene object "NetworkLobby"
/// </summary>
public class RoomPanel : MonoBehaviour
{
    [SerializeField] private PlayerPanel playerPanelPrefab;
    [SerializeField] private Transform playerPanelParent1;
    [SerializeField] private Transform playerPanelParent2;
    //[SerializeField] private TMP_Text _waitingText;
    [SerializeField] private GameObject startButton, readyButton;

    private readonly List<PlayerPanel> playerPanels = new();
    private bool allReady;
    private bool ready;
    private int teamSetter;

    public static event Action StartPressed;

    private void OnEnable()
    {
        foreach (Transform child in playerPanelParent1) Destroy(child.gameObject);
        foreach (Transform child in playerPanelParent2) Destroy(child.gameObject);
        playerPanels.Clear();

        LobbyManager.LobbyPlayersUpdated += NetworkLobbyPlayersUpdated;
        MatchmakingService.CurrentLobbyRefreshed += OnCurrentLobbyRefreshed;
        startButton.SetActive(false);
        readyButton.SetActive(false);

        ready = false;
    }

    private void OnDisable()
    {
        LobbyManager.LobbyPlayersUpdated -= NetworkLobbyPlayersUpdated;
        MatchmakingService.CurrentLobbyRefreshed -= OnCurrentLobbyRefreshed;
    }

    public static event Action LobbyLeft;

    public void OnLeaveLobby()
    {
        LobbyLeft?.Invoke();
    }

    private void NetworkLobbyPlayersUpdated(Dictionary<ulong, bool> players)
    {
        var allActivePlayerIds = players.Keys;
        teamSetter = (players.Count % 2 == 1) ? 1 : 2;

        // Remove all inactive panels
        var toDestroy = playerPanels.Where(p => !allActivePlayerIds.Contains(p.PlayerId)).ToList();
        foreach (var panel in toDestroy)
        {
            playerPanels.Remove(panel);
            Destroy(panel.gameObject);
        }

        foreach (var player in players)
        {
            var currentPanel = playerPanels.FirstOrDefault(p => p.PlayerId == player.Key);
            if (currentPanel != null)
            {
                if (player.Value) currentPanel.SetReady();
            }
            else
            {
                if(teamSetter == 1)
                {
                    var panel = Instantiate(playerPanelPrefab, playerPanelParent1);
                    panel.Init(player.Key, teamSetter);
                    playerPanels.Add(panel);
                }
                else
                {
                    var panel = Instantiate(playerPanelPrefab, playerPanelParent2);
                    panel.Init(player.Key, teamSetter);
                    playerPanels.Add(panel);
                }
            }
        }

        startButton.SetActive(NetworkManager.Singleton.IsHost && players.All(p => p.Value));
        readyButton.SetActive(!ready);
    }

    private void OnCurrentLobbyRefreshed(Lobby lobby)
    {
        //waitingText.text = $"Waiting on players... {lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void OnReadyClicked()
    {
        readyButton.SetActive(false);
        ready = true;
    }

    public void OnStartClicked()
    {
        StartPressed?.Invoke();
    }
}
