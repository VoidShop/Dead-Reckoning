using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, teamText;
    [SerializeField] private Toggle readyToggle;

    public ulong PlayerId { get; private set; }
    public int TeamId { get; private set; }

    public void Init(ulong playerId, int teamId)
    {
        PlayerId = playerId;
        TeamId = teamId;
        nameText.text = $"Player {playerId}";
        teamText.text = $"{teamId}";
    }

    public void SetReady()
    {
        readyToggle.gameObject.SetActive(true);
        readyToggle.isOn = true;
    }
}
