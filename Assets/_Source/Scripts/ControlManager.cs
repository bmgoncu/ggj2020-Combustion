using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ControlManager : SingletonComponent<ControlManager>
{

    public GameObject playerPrefab;
    public HudUI timer;
    public string[] colors= {"red", "green", "blue", "purple"};


    private static int success = 0;
    private const int MAX_PLAYER_COUNT = 2;
    private int playerCounter = 0;



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
            playerCounter++;
            if (AirConsole.instance.GetMasterControllerDeviceId() == deviceId)
            {
                AirConsole.instance.Message(deviceId, "MASTER");
            }
            if (playerCounter < 5)
            {
                AirConsole.instance.Message(deviceId, "s." + colors[playerCounter-1]);
                foreach(int key in AirConsole.instance.GetControllerDeviceIds())
                {
                    if (key != deviceId)
                    {
                        AirConsole.instance.Message(key, "t." + colors[playerCounter-1]);
                    }
                }
            }
            /*
            if (AirConsole.instance.GetControllerDeviceIds().Count >= MAX_PLAYER_COUNT)
            {
                Debug.Log("heyo");
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
        playerCounter--;
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
        if (from == AirConsole.instance.GetMasterControllerDeviceId() && data.ToString() == "START")
        {
            StartGame(AirConsole.instance.GetControllerDeviceIds().Count);
        }
        int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber(from);

        if  (active_player == -1 || StageManager.Instance.PlayersDic == null || !StageManager.Instance.PlayersDic.ContainsKey(from))
        {
            return;
        }

        if (data.ToString() == "START")
        {
            return;
        }

        StageManager.Instance.PlayersDic[from].Move(2f*new Vector2((float)data["x"], (float)data["y"]));

        if ((int)data["action"] == 1)
        {
            bool flag = false;
            if (!StageManager.Instance.PlayersDic[from].Board)
            {
                foreach (Ship ship in StageManager.Instance.SceneShips)
                {
                    if (Vector3.Distance(ship.transform.position, StageManager.Instance.PlayersDic[from].transform.position) < 3f)
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
                            StageManager.Instance.PlayersDic[from].OnBoard(ship);
                            flag = true;
                        }
                        break;
                    }
                }
            }
            else
            {
                Escape(StageManager.Instance.PlayersDic[from].Board);
                flag = true;
            }
            Debug.Log(flag);
            if (!flag)
            {
                StageManager.Instance.PlayersDic[from].DoAction();
            }
        }
    }

    public void StartGame(int playerCount)
    {
        if (StageManager.Instance.PlayersDic == null)
        {
            AirConsole.instance.SetActivePlayers(Mathf.Clamp(playerCount, MAX_PLAYER_COUNT, 4));
            var playerIds = AirConsole.instance.GetActivePlayerDeviceIds;
            StageManager.Instance.CreatePlayers(playerIds.ToList());
        }
        StageManager.Instance.Generate(playerCount);
        foreach(int key in StageManager.Instance.PlayersDic.Keys)
        {
            AirConsole.instance.Message(key, "STARTING");
        }
        timer.CountDown(41);
    }

    public void Escape(Ship ship)
    {
        foreach(Player player in StageManager.Instance.PlayersDic.Values)
        {
            if (player.Board == ship)
            {
                AirConsole.instance.Message(player.Id, "ESCAPEATTEMPT");
            }
        }
        ship.transform.DOMove(transform.position + Vector3.up * 25f, 2f).OnComplete(()=> {
            success++;
            ship.transform.position = new Vector3(-3f * success, 25f, -5f);
        });
    }

    public void Death()
    {
        foreach(int key in StageManager.Instance.PlayersDic.Keys)
        {
            AirConsole.instance.Message(key, "END");
        }

        foreach(Player player in StageManager.Instance.PlayersDic.Values)
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
        foreach (int key in StageManager.Instance.PlayersDic.Keys)
        {
            foreach (var ship in StageManager.Instance.SceneShips)
            {
                ship.UpdateStatusUI(true);
            }
            StageManager.Instance.PlayersDic[key].transform.position = Vector3.zero;
            AirConsole.instance.Message(key, "RESTART");
        }
        success = 0;
    }
}