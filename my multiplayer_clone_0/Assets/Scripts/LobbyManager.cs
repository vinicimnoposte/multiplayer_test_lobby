//using System.Collections.Generic;
//using System.Threading.Tasks;
//using TMPro;
//using Unity.Services.Authentication;
//using Unity.Services.Core;
//using Unity.Services.Lobbies;
//using Unity.Services.Lobbies.Models;
//using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
//using Unity.Services.Relay.Models;
//using Unity.Services.Relay;
//using Unity.Networking.Transport.Relay;
//using Unity.Netcode;
//using Unity.Netcode.Transports.UTP;

public class LobbyManager : MonoBehaviour
{
    public TMP_InputField playerNameInput, lobbyCodeInput;
    public GameObject introLobby, lobbyPanel;
    public TMP_Text[] lobbyPlayersText;
    public TMP_Text lobbyCodeText;
    public Lobby hostLobby, joinnedLobby;
    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    async Task Authenticate()
    {
        if (AuthenticationService.Instance.IsSignedIn)
            return;
        else
        {
            Debug.Log("player nao logou");
        }

        AuthenticationService.Instance.ClearSessionToken();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("usuario logado como " + AuthenticationService.Instance.PlayerId);
    }

    async public void CreateLobby()
    {
        await Authenticate();

        CreateLobbyOptions options = new CreateLobbyOptions
        {

            Player = GetPlayer()
            
        };
        hostLobby = await Lobbies.Instance.CreateLobbyAsync("lobby", 4, options);
        joinnedLobby = hostLobby;
        Debug.Log("criou o lobby " + hostLobby.LobbyCode);
        InvokeRepeating("SendLobbyHeartBeat", 10, 10);
        ShowPlayers();
        lobbyCodeText.text = joinnedLobby.LobbyCode;
        introLobby.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    async public void JoinLobbyByCode()
    {
        await Authenticate();

        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = GetPlayer()     
        };

        joinnedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCodeInput.text, options);
        Debug.Log("Entrou no lobby " + joinnedLobby.LobbyCode);
        ShowPlayers();
        lobbyCodeText.text = joinnedLobby.LobbyCode;
        introLobby.SetActive(false);
        lobbyPanel.SetActive(true);
    }

     Player GetPlayer()
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerNameInput.text) }
            }
        };
        if (player == null)
        {
            Debug.Log("player é nulo");
        }
        return player;
        

    }

    async void SendLobbyHeartBeat()
    {
        if (hostLobby == null)
            return;

        await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        Debug.Log("Atualizou o lobby");
        UpdateLobby();
        ShowPlayers();
    }



    async void UpdateLobby()
    {
        if (joinnedLobby == null)
            return;

        joinnedLobby = await LobbyService.Instance.GetLobbyAsync(joinnedLobby.Id);
    }
    void ShowPlayers()
    {
        for (int i = 0; i < joinnedLobby.Players.Count; i++)
        {
            print(lobbyPlayersText);
            print(joinnedLobby.Players[i].Data);
            lobbyPlayersText[i].text = joinnedLobby.Players[i].Data["name"].Value;
            

        }
    }
}
