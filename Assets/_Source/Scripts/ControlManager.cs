using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    static ControlManager _instance;

    public static ControlManager Instance
    {
        get
        {
            return _instance ? _instance : _instance = FindObjectOfType<ControlManager>();
        }
    }

    static int success = 0;

    [SerializeField] GameObject playerPrefab;

    [SerializeField] HudUI timer;

    public Dictionary<int,Player> PlayersDic { get; private set; }

    private const int MAX_PLAYER_COUNT = 2;

    int playerCounter = 0;

    public string[] colors= {"red", "green", "blue", "purple"};

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

        if  (active_player == -1 || PlayersDic == null || !PlayersDic.ContainsKey(from))
        {
            return;
        }

        if (data.ToString() == "START")
        {
            return;
        }

        PlayersDic[from].Move(2f*new Vector2((float)data["x"], (float)data["y"]));

        if ((int)data["action"] == 1)
        {
            PlayersDic[from].DoAction();
        }
        if ((int)data["action"] == 2)
        {
            if (!PlayersDic[from].Board)
            {
                foreach (Ship ship in FindObjectsOfType<Ship>())
                {
                    if (Vector3.Distance(ship.transform.position, PlayersDic[from].transform.position) < 2f)
                    {
                        int ftc = 0, orc = 0, enc = 0;
                        for (int i = 0; i < ship.transform.childCount; i++)
                        {
                            if (ship.transform.GetChild(i).GetComponent<ShipComponent>().GetShipComponentType() == ShipComponentType.ENGINE)
                            {
                                enc++;
                            }
                            if (ship.transform.GetChild(i).GetComponent<ShipComponent>().GetShipComponentType() == ShipComponentType.FUEL_TANK)
                            {
                                ftc++;
                            }
                            if (ship.transform.GetChild(i).GetComponent<ShipComponent>().GetShipComponentType() == ShipComponentType.ORBITER)
                            {
                                orc++;
                            }
                        }
                        if (enc != 0 && orc != 0 && ftc != 0)
                        {
                            PlayersDic[from].OnBoard(ship);
                        }
                        break;
                    }
                }
            }
            else
            {
                Escape(PlayersDic[from].Board);
            }
        }
    }

    public void StartGame(int playerCount)
    {
        if (PlayersDic == null)
        {
            PlayersDic = new Dictionary<int, Player>();
            AirConsole.instance.SetActivePlayers(Mathf.Clamp(playerCount, MAX_PLAYER_COUNT, 4));
            var playerIds = AirConsole.instance.GetActivePlayerDeviceIds;
            for (int i = 0; i < playerIds.Count; i++)
            {
                PlayersDic[playerIds[i]] = Instantiate(playerPrefab,
                    transform.GetChild(i).position,
                    Quaternion.identity, StageManager.Instance.transform)
                    .GetComponent<Player>();
                PlayersDic[playerIds[i]].Id = playerIds[i];
                PlayersDic[playerIds[i]].SetColor(GetColorForIndex(i));
            }
        }
        StageManager.Instance.Generate(playerCount);
        foreach(int key in PlayersDic.Keys)
        {
            AirConsole.instance.Message(key, "STARTING");
        }
        timer.CountDown(21);
    }

    private Color GetColorForIndex(int i)
    {
        switch (i)
        {
            case 0:
                return Color.red;
            case 1:
                return Color.green;
            case 2:
                return Color.blue;
            case 3:
                return Color.magenta;
            default:
                return Color.black;
        }
    }

    public void Escape(Ship ship)
    {
        foreach(Player player in PlayersDic.Values)
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
        foreach(int key in PlayersDic.Keys)
        {
            AirConsole.instance.Message(key, "END");
        }

        foreach(Player player in PlayersDic.Values)
        {
            player.PhysicsRigidBody.velocity = Vector3.zero;
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
                foreach (var ship in FindObjectsOfType<Ship>())
                {
                    ship.StatusUI.gameObject.SetActive(false);
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
        foreach (int key in PlayersDic.Keys)
        {
            PlayersDic[key].transform.position = Vector3.zero;
            AirConsole.instance.Message(key, "RESTART");
        }
        success = 0;
    }
}