using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ControlManager : SingletonComponent<ControlManager>
{
    const int _MIN_PLAYER_COUNT_ = 2;
    const int _MAX_PLAYER_COUNT_ = 4;

    [SerializeField] HudUI _hudUI;

    public List<int> InactivePlayers { get; private set; } = new List<int>();

    public Dictionary<int, Player> ActivePlayers { get; private set; } = new Dictionary<int, Player>();

    bool _refreshLobbyNextRound; // sometimes we need to refresh the lobby.

    int _winnersCount = -1; // when it's -1, that means we're in the lobby.

    protected override void Awake()
    {
        base.Awake();

        AirConsole.instance.onConnect += OnConnect;
        AirConsole.instance.onMessage += OnMessage;
        AirConsole.instance.onDisconnect += OnDisconnect;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (AirConsole.instance != null)
        {
            AirConsole.instance.onMessage -= OnMessage;
        }
    }

    void OnConnect(int deviceId)
    {
        if (_winnersCount == -1)
        {
            if (ActivePlayers.Count < 4)
            {
                ActivePlayers[deviceId] = StageManager.Instance.CreatePlayer(deviceId, FindIndex());
            }
            else
            {
                InactivePlayers.Add(deviceId);
            }

            LobbyUpdate(deviceId);
        }
        else
        {
            InactivePlayers.Add(deviceId);
            _refreshLobbyNextRound = true;
        }
    }

    void OnDisconnect(int deviceId)
    {
        if (ActivePlayers.ContainsKey(deviceId))
        {
            if (InactivePlayers.Count > 0)
            {
                int newId = InactivePlayers[0];
                InactivePlayers.RemoveAt(0);

                Player player = ActivePlayers[deviceId];
                ActivePlayers.Remove(deviceId);

                player.Initialize(newId);
                ActivePlayers.Add(newId, player);

                if (_winnersCount != -1)
                {
                    player.gameObject.SetActive(false);
                }
            }
            else
            {
                Destroy(ActivePlayers[deviceId].gameObject);
                ActivePlayers.Remove(deviceId);
            }
        }

        if (InactivePlayers.Contains(deviceId))
        {
            InactivePlayers.Remove(deviceId);
        }

        if (_winnersCount == -1)
        {
            LobbyUpdate();
        }
        else
        {
            _refreshLobbyNextRound = true;
        }
    }

    // TODO: bu metotta bitmiş gemiye biniyor mu kontrolü var.
    // Yakındaki geminin alındığı fonksiyona taşınmalı.
    void OnMessage(int from, JToken data)
    {
        if (from == AirConsole.instance.GetMasterControllerDeviceId())
        {
            if (data.ToString().Equals("START"))
            {
                InitGame();
                return;
            }
        }

        if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0 || !ActivePlayers.ContainsKey(from))
        {
            return;
        }

        ActivePlayers[from].Move(new Vector2((float)data["x"], (float)data["y"]));

        // we can send action as true/false again.
        if ((int)data["action"] == 1)
        {
            bool flag = false;

            if (!ActivePlayers[from].Board)
            {
                foreach (Ship ship in StageManager.Instance.SceneShips)
                {
                    if (Vector3.Distance(ship.transform.position, ActivePlayers[from].transform.position) < 3f)
                    {
                        int ftc = 0, orc = 0, enc = 0;
                        foreach (ShipComponent shipComponent in ship.CurrentComponents)
                        {
                            if (shipComponent.Type == ShipComponentType.ENGINE)
                            {
                                enc++;
                            }
                            if (shipComponent.Type == ShipComponentType.FUEL_TANK)
                            {
                                ftc++;
                            }
                            if (shipComponent.Type == ShipComponentType.ORBITER)
                            {
                                orc++;
                            }
                        }
                        if (enc != 0 && orc != 0 && ftc != 0)
                        {
                            ActivePlayers[from].OnBoard(ship);
                            flag = true;
                        }
                        break;
                    }
                }
            }
            else
            {
                Escape(ActivePlayers[from].Board);
                flag = true;
            }

            if (!flag)
            {
                ActivePlayers[from].DoAction();
            }
        }
    }

    int FindIndex()
    {
        HashSet<int> indexes = new HashSet<int> { 0, 1, 2, 3 };

        foreach(Player player in ActivePlayers.Values)
        {
            indexes.Remove(player.Index);
        }

        int ret = 4;
        foreach (int index in indexes)
        {
            if (index < ret)
            {
                ret = index;
            }
        }
        return ret;
    }

    void LobbyUpdate()
    {
        AirConsole.instance.Broadcast("CLEAR");

        foreach (int id in AirConsole.instance.GetControllerDeviceIds())
        {
            LobbyUpdate(id);
        }
    }

    void LobbyUpdate(int deviceId)
    {
        if (AirConsole.instance.GetMasterControllerDeviceId() == deviceId)
        {
            AirConsole.instance.Message(deviceId, "MASTER");
            UIManager.Instance.ShowPanel(UIPanelType.Lobby, new LobbyPanelData());
        }

        string color = "gray";
        if (ActivePlayers.ContainsKey(deviceId))
        {
            color = ActivePlayers[deviceId].GetColor();
        }

        List<int> deviceIds = AirConsole.instance.GetControllerDeviceIds();

        foreach (int did in deviceIds)
        {
            if (deviceId != did)
            {
                AirConsole.instance.Message(did, "t." + color);

                string c = "gray";
                if (ActivePlayers.ContainsKey(did))
                {
                    c = ActivePlayers[did].GetColor();
                }
                AirConsole.instance.Message(deviceId, "t." + c);
            }
        }

        AirConsole.instance.Message(deviceId, "s." + color);
    }

    /* InitGame triggers countdown in UI */

    void InitGame()
    {
        _winnersCount = 0;

        UIManager.Instance.HidePanel(UIPanelType.Lobby);
        
        Camera.main.transform.DOMove(new Vector3(0f, 7f, -5f), 1f);
        Camera.main.transform.DORotate(70f * Vector3.right, 1f).OnComplete(() => {
            _hudUI.StartCountdown(3);
        });

        foreach (int did in ActivePlayers.Keys)
        {
            AirConsole.instance.Message(did, "STARTING");
        }

        StageManager.Instance.Generate(ActivePlayers.Count);
    }

    /* When the countdown is over, UI triggers InitGame - that's why it is public */

    public void StartGame()
    {
        AirConsole.instance.SetActivePlayers(Mathf.Clamp(ActivePlayers.Count, _MIN_PLAYER_COUNT_, _MAX_PLAYER_COUNT_));
        foreach (var ship in StageManager.Instance.SceneShips)
        {
            ship.UpdateStatusUI(true);
        }

        _hudUI.StartTimer(31);
        Distraction.Instance.EnableDistraction();
    }

    void Escape(Ship ship)
    {
        foreach (Player player in ActivePlayers.Values)
        {
            if (player.Board == ship)
            {
                AirConsole.instance.Message(player.Id, "ESCAPEATTEMPT");
            }
        }
        ship.transform.DOMove(transform.position + Vector3.up * 25f, 2f).OnComplete(() =>
        {
            _winnersCount++;
            ship.transform.position = new Vector3(-3f * _winnersCount, 25f, -5f);
        });
    }

    /* when the round timer is over, UI triggers Death */

    public void Death()
    {
        Distraction.Instance.DisableDistraction();

        foreach (int key in ActivePlayers.Keys)
        {
            AirConsole.instance.Message(key, "END");
        }

        foreach (var ship in StageManager.Instance.SceneShips)
        {
            ship.UpdateStatusUI(false);
        }

        foreach (Player player in ActivePlayers.Values)
        {
            player.ResetPlayer();
        }

        StartCoroutine(EndScreen());
    }

    private int EndScreenCountdown = 7;
    IEnumerator EndScreen()
    {
        List<Ship> survivingShips = new List<Ship>();
        foreach (Ship ship in StageManager.Instance.SceneShips)
        {
            if (Random.Range(0, 1) > ship.SetTotalChancePoint())
            {
                // Exploded
            }
            else
            {
                survivingShips.Add(ship);
            }
        }

        UIManager.Instance.ShowPanel(UIPanelType.Result, new ResultPanelData()
        {
            IsWin = survivingShips.Count > 0,
            VictoriousPlayers = survivingShips,
            CountDown = EndScreenCountdown
        });
        
        Camera.main.transform.DOMove(new Vector3(0f, 25f, -15f), 1f);
        Camera.main.transform.DORotate(Vector3.zero, 1f).OnComplete(() =>
        {
            
            Sequence cinamatic = DOTween.Sequence();
            cinamatic.AppendInterval(1.5f);
            cinamatic.AppendCallback(() =>
            {
                foreach (Ship ship in StageManager.Instance.SceneShips)
                {
                    ship.UpdateStatusUI(false);
                    if (!survivingShips.Contains(ship))
                    {
                        ship.Explode();
                        ship.transform.DOMoveY(10, 2);
                    }
                    else
                    {
                        ship.transform.DOMoveY(50, 2);
                    }
                }
            });
        });

        yield return new WaitForSeconds(EndScreenCountdown);

        StageManager.Instance.Clean();

        Camera.main.transform.DOMove(new Vector3(0f, 0.07f, -5f), 1f);
        Camera.main.transform.DORotate(19f * Vector3.left, 1f);

        _winnersCount = -1;
        AirConsole.instance.SetActivePlayers(0);

        foreach (Player player in ActivePlayers.Values)
        {
            player.gameObject.SetActive(true);
            player.ResetPlayer();
            StageManager.Instance.ResetPlayerTransform(player);
            AirConsole.instance.Message(player.Id, "RESTART");
        }

        if (_refreshLobbyNextRound)
        {
            if (InactivePlayers.Count > 0)
            {
                for(int i=0; i<InactivePlayers.Count && ActivePlayers.Count <= 4; i++)
                {
                    int player = InactivePlayers[i];
                    InactivePlayers.Remove(player);
                    OnConnect(player);
                }
            }
            _refreshLobbyNextRound = false;
            LobbyUpdate();
        }

        _winnersCount = 0;
    }
}
