using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

public class PlayerListItemUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image colorImage;
    [SerializeField] GameObject kickButtonObject;

    string playerId;

    private void Start()
    {
/*        nameText.text = "Player Name";
        colorImage.color = Color.white;
        kickButtonObject.SetActive(false);*/
    }

    public void SetUp(Player player)
    {
        if (player == null) return;
        nameText.text = player.Data["PlayerName"].Value;

        string hexColor = player.Data["Color"].Value;
        Color imageColor;
        if (ColorUtility.TryParseHtmlString(hexColor, out imageColor))
        {
            colorImage.color = imageColor;
        }

        kickButtonObject.SetActive(LobbyManager.Instance.IsPlayerHost());

        playerId = player.Id;
    }


    public void KickPlayer()
    {
        LobbyManager.Instance.KickPlayer(playerId);
    }
    
}
