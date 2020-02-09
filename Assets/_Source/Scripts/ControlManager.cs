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

    bool _refreshLobbyNextRound;

    int _winnersCount = 0;

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
        if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0)
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

                player.Id = newId;
                ActivePlayers.Add(newId, player);

                if (AirConsole.instance.GetActivePlayerDeviceIds.Count != 0)
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

        if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0)
        {
            LobbyUpdate();
        }
        else
        {
            _refreshLobbyNextRound = true;
        }
    }

    void OnMessage(int from, JToken data)
    {
        if (from == AirConsole.instance.GetMasterControllerDeviceId() && data.ToString() == "START")
        {
            StartGame();
            return;
        }

        if  (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0 || !ActivePlayers.ContainsKey(from))
        {
            return;
        }

        ActivePlayers[from].Move(new Vector2((float)data["x"], (float)data["y"]));

        // we can send action as true/false again.
        if ((int)data["action"] == 1)
        {
            bool flag = false;

            // we can store approached item in the player script...
            if (!ActivePlayers[from].Board)
            {
                foreach (Ship ship in StageManager.Instance.SceneShips)
                {
                    if (Vector3.Distance(ship.transform.position, ActivePlayers[from].transform.position) < 3f)
                    {
                        int ftc = 0, orc = 0, enc = 0;
                        for (int i = 0; i < ship.transform.childCount; i++)
                        {
                            if (ship.transform.GetChild(i).GetComponent<ShipComponent>().Type == ShipComponentType.ENGINE)
                            {
                                enc++;
                            }
                            if (ship.transform.GetChild(i).GetComponent<ShipComponent>().Type == ShipComponentType.FUEL_TANK)
                            {
                                ftc++;
                            }
                            if (ship.transform.GetChild(i).GetComponent<ShipComponent>().Type == ShipComponentType.ORBITER)
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
                StageManager.Instance.PlayersDic[from].DoAction();
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
        }

        string color = "gray";
        if (ActivePlayers.ContainsKey(deviceId))
        {
            color = GetColorString(ActivePlayers[deviceId].color);
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
                    c = GetColorString(ActivePlayers[did].color);
                }
                AirConsole.instance.Message(deviceId, "t." + c);
            }
        }

        AirConsole.instance.Message(deviceId, "s." + color);
    }

    void StartGame()
    {
        Camera.main.transform.DOMove(new Vector3(0f, 7f, -5f), 1f);
        Camera.main.transform.DORotate(70f * Vector3.right, 1f).OnComplete(() => {
            _hudUI.StartCountdown(3);
        });
        foreach (int did in ActivePlayers.Keys)
        {
            AirConsole.instance.Message(did, "STARTING");
        }
    }

    public void InitGame()
    {
        AirConsole.instance.SetActivePlayers(Mathf.Clamp(ActivePlayers.Count, _MIN_PLAYER_COUNT_, _MAX_PLAYER_COUNT_));
        _hudUI.StartTimer(61);
    }

    string GetColorString(Color c)
    {
        if (c.Equals(Color.red))
        {
            return "red";
        }
        if (c.Equals(Color.green))
        {
            return "green";
        }
        if (c.Equals(Color.blue))
        {
            return "blue";
        }
        if (c.Equals(Color.magenta))
        {
            return "purple";
        }
        return "gray";
    }

    public void Escape(Ship ship)
    {
        foreach(Player player in ActivePlayers.Values)
        {
            if (player.Board == ship)
            {
                AirConsole.instance.Message(player.Id, "ESCAPEATTEMPT");
            }
        }
        ship.transform.DOMove(transform.position + Vector3.up * 25f, 2f).OnComplete(()=> {
            _winnersCount++;
            ship.transform.position = new Vector3(-3f * _winnersCount, 25f, -5f);
        });
    }

    public void Death()
    {
        foreach(int key in ActivePlayers.Keys)
        {
            AirConsole.instance.Message(key, "END");
        }

        foreach(Player player in ActivePlayers.Values)
        {
            player.PhysicsRigidBody.velocity = Vector3.zero;
        }

        foreach (var ship in StageManager.Instance.SceneShips)
        {
            ship.UpdateStatusUI(false);
        }

        StartCoroutine(EndScreen());
    }

    IEnumerator EndScreen()
    {
        Camera.main.transform.DOMove(new Vector3(0f, 25f, -15f), 1f);
        Camera.main.transform.DORotate(Vector3.zero, 1f).OnComplete(() =>
        {
            Sequence cinamatic = DOTween.Sequence();
            cinamatic.AppendInterval(1.5f);
            cinamatic.AppendCallback(() => {
                foreach (var ship in StageManager.Instance.SceneShips)
                {
                    ship.UpdateStatusUI(false);
                    if (Random.Range(0, 1) > ship.SetTotalChancePoint())
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
        yield return new WaitForSeconds(7f);
        StageManager.Instance.Clean();
        Camera.main.transform.DOMove(new Vector3(0f, 7f, -5f), 1f);
        Camera.main.transform.DORotate(70f * Vector3.right, 1f);
        yield return new WaitForSeconds(3f);

        foreach (int key in ActivePlayers.Keys)
        {
            foreach (var ship in StageManager.Instance.SceneShips)
            {
                ship.UpdateStatusUI(true);
            }
            StageManager.Instance.PlayersDic[key].transform.position = Vector3.zero;
            AirConsole.instance.Message(key, "RESTART");
        }
        foreach(Player player in ActivePlayers.Values)
        {
            player.gameObject.SetActive(true);
            StageManager.Instance.ResetPlayerTransform(player);
        }
        if (_refreshLobbyNextRound)
        {
            _refreshLobbyNextRound = false;
            LobbyUpdate();
        }
        _winnersCount = 0;
    }
}