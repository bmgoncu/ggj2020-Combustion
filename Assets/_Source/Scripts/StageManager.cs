using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    static StageManager _instance;
    public static StageManager Instance
    {
        get
        {
            return _instance ? _instance : _instance = FindObjectOfType<StageManager>();
        }
    }

    static readonly Vector3 EXTREMES = new Vector3(5f, -6f, 2f);

    const float _MIN_DIST_ = 10f;
    const float _STEP_ = 0.05f;

    [SerializeField] GameObject[] Engine;
    [SerializeField] GameObject[] FuelTank;
    [SerializeField] GameObject[] Toilet;

    readonly List<GameObject> _instantiated = new List<GameObject>();

    List<Vector3> _players = new List<Vector3>();

    public void Generate(int playerCount)
    {
        foreach (Player player in FindObjectsOfType<Player>())
        {
            _players.Add(player.transform.position);
        }
        PlaceObject(Engine, playerCount);
        PlaceObject(FuelTank, playerCount);
        PlaceObject(Toilet, playerCount);

        /*
        int counter = 0;
        foreach (Ship ship in FindObjectsOfType<Ship>())
        {
            ship.OwnerId = ++counter;
        }
        */
    }

    public void PlaceObject(GameObject[] obj, int count)
    {
        count += Random.Range(0, 2);
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPosition;
            float dist = _MIN_DIST_;
            do
            {
                randomPosition = new Vector3(Random.Range(-EXTREMES.x, EXTREMES.x), 0.2f, Random.Range(EXTREMES.y, EXTREMES.z));
                if (Valid(dist, randomPosition))
                {
                    _instantiated.Add(Instantiate(obj[Random.Range(0,obj.Length)], randomPosition, Quaternion.identity, transform));
                }
                else
                {
                    dist -= _STEP_;
                    randomPosition = Vector3.negativeInfinity;
                }
            }
            while (randomPosition.Equals(Vector3.negativeInfinity));
        }
    }

    public bool Valid(float dist, Vector3 pos)
    {
        foreach(Vector3 playerpos in _players)
        {
            if (Vector3.Distance(pos, playerpos) < dist)
            {
                return false;
            }
        }
        foreach(GameObject go in _instantiated)
        {
            if (Vector3.Distance(pos, go.transform.position) < dist)
            {
                return false;
            }
        }
        return true;
    }

    public void Clean()
    {
        for (int i=_instantiated.Count-1; i>=0; i--)
        {
            Destroy(_instantiated[i]);
        }

        _players.Clear();
        _instantiated.Clear();

        foreach(Ship ship in FindObjectsOfType<Ship>())
        {
            ship.ResetShip();
        }

        foreach (Player player in FindObjectsOfType<Player>())
        {
            player.ResetPlayer();
        }
    }
}
