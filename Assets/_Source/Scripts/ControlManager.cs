using System.Collections.Generic;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    public PlayerState[] PlayerStates { get; private set; } 

    public Dictionary<int,Player> PlayersDic { get; private set; }

    private const int MAX_PLAYER_COUNT = 2;

    void Awake()
    {
        AirConsole.instance.onMessage += OnMessage;
        AirConsole.instance.onConnect += OnConnect;
        AirConsole.instance.onDisconnect += OnDisconnect;
    }

    void OnDestroy()
    {
        if (AirConsole.instance != null)
        {
            AirConsole.instance.onMessage -= OnMessage;
        }
    }

    void OnConnect(int deviceId)
    {
        if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0)
        {
            if (AirConsole.instance.GetMasterControllerDeviceId() == deviceId)
            {
                AirConsole.instance.Message(deviceId, "MASTER");
            }
            /*
            if (AirConsole.instance.GetControllerDeviceIds().Count >= MAX_PLAYER_COUNT)
            {
                StartGame(AirConsole.instance.GetControllerDeviceIds().Count);
            }
            else
            {
                // Bir şey yapma - eksik oyuncuyla oyunu başlatma tuşunu beklet.
            }
            */
        }
    }

    void OnDisconnect(int deviceId)
    {
        int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber(deviceId);
        if (active_player != -1)
        {
            if (AirConsole.instance.GetControllerDeviceIds().Count >= 4)
            {
                // Start Game - gerekli mi belli değil.
            }
            else
            {
                // AirConsole.instance.SetActivePlayers(0);
                // Oyun yok ya da oyun devam...
            }
        }
    }

    void OnMessage(int from, JToken data)
    {
        int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber(from);

        if  (active_player == -1 || PlayersDic == null || !PlayersDic.ContainsKey(from))
        {
            return;
        }

        PlayersDic[from].Move(new Vector2((float)data["x"], (float)data["y"]));

        if ((bool)data["action"])
        {
            PlayersDic[from].DoAction();
        }
    }

    public void StartGame(int playerCount)
    {
        PlayersDic = new Dictionary<int, Player>();
        AirConsole.instance.SetActivePlayers(playerCount);
        var playerIds = AirConsole.instance.GetActivePlayerDeviceIds;
        for (int i = 0; i < playerIds.Count; i++)
        {
            PlayersDic[playerIds[i]] = Instantiate(playerPrefab,
                transform.GetChild(i).position,
                Quaternion.identity)
                .GetComponent<Player>();
            PlayersDic[playerIds[i]].Id = playerIds[i];
        }
        PlayerStates = new PlayerState[playerCount];
        StageManager.Instance.Generate(playerCount);
    }
}

public struct PlayerState
{
    bool right, down, left, up;
}