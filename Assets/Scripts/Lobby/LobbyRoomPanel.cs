using TMPro;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using System;

public class LobbyRoomPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, playerCountText;

    public Lobby Lobby { get; private set; }

    public static event Action<Lobby> LobbySelected;

    public void Init(Lobby lobby)
    {
        UpdateDetails(lobby);
    }

    public void UpdateDetails(Lobby lobby)
    {
        Lobby = lobby;
        nameText.text = lobby.Name;

        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

        int GetValue(string key)
        {
            return int.Parse(lobby.Data[key].Value);
        }
    }

    public void Clicked()
    {
        LobbySelected?.Invoke(Lobby);
    }
}
