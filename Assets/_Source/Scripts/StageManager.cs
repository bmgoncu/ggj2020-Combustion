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

    static readonly Vector3 EXTREMES = new Vector3(10f, 0f, 10f);

    const float _MIN_DIST_ = 10f;
    const float _STEP_ = 0.05f;

    [SerializeField] GameObject Ship;

    [SerializeField] GameObject Engine;
    [SerializeField] GameObject FuelTank;
    [SerializeField] GameObject Toilet;

    readonly List<GameObject> _instantiated = new List<GameObject>();

    List<Vector3> _players = new List<Vector3>();

    public void Generate(int playerCount)
    {
        foreach(Player player in FindObjectsOfType<Player>())
        {
            _players.Add(player.transform.position);
        }
        PlaceObject(Ship, playerCount);
        PlaceObject(Engine, playerCount);
        PlaceObject(FuelTank, playerCount);
        PlaceObject(Toilet, playerCount);

        int counter = 0;
        foreach(Ship ship in FindObjectsOfType<Ship>())
        {
            ship.OwnerId = ++counter;
        }
    }

    public void PlaceObject(GameObject obj, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPosition;
            float dist = _MIN_DIST_;
            do
            {
                randomPosition = new Vector3(Random.Range(-EXTREMES.x, EXTREMES.x), 0f, Random.Range(-EXTREMES.z, EXTREMES.z));
                if (Valid(dist, randomPosition))
                {
                    _instantiated.Add(Instantiate(obj, randomPosition, Quaternion.identity));
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
}
