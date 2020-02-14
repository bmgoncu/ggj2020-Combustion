using System.Collections.Generic;
using UnityEngine;

/*  TODO: Bu sınıf güzel ama pooling yapmıyor - Soru yapmalı mı? */
public class StageManager : SingletonComponent<StageManager>
{
    /* update following value */
    readonly Vector3 EXTREMES = new Vector3(5f, -5f, 0.3f);
    /* update the value above */

    public List<Ship> SceneShips;

    public GameObject PlayerPrefab;

    public GameObject[] Engine;
    public GameObject[] Toilet;
    public GameObject[] FuelTank;

    readonly List<GameObject> _instantiated = new List<GameObject>();

    public Player CreatePlayer(int playerId, int index)
    {
        Player player = Instantiate(
            PlayerPrefab,
            transform.GetChild(index).position,
            transform.GetChild(index).rotation,
            transform
        ).GetComponent<Player>();

        player.Initialize(playerId, index);

        return player;
    }

    public void ResetPlayerTransform(Player player)
    {
        player.transform.SetParent(transform);
        player.transform.position = transform.GetChild(player.Index).position;
        player.transform.rotation = transform.GetChild(player.Index).rotation;
    }

    public void Generate(int playerCount)
    {
        PlaceObject(Engine, playerCount);
        PlaceObject(FuelTank, playerCount);
        PlaceObject(Toilet, playerCount);
    }

    void PlaceObject(GameObject[] obj, int count)
    {
        count += Random.Range(0, 2);

        for (int i = 0; i < count; i++)
        {
            Vector3 position;
            float dist = 10f;
            do
            {
                position = new Vector3(Random.Range(-EXTREMES.x, EXTREMES.x), 0.2f, Random.Range(EXTREMES.y, EXTREMES.z));
                if (Valid(dist, position))
                {
                    _instantiated.Add(Instantiate(obj[Random.Range(0,obj.Length)], position, Quaternion.identity, transform));
                }
                else
                {
                    dist -= 0.025f;
                    position = Vector3.negativeInfinity;
                }
            }
            while (position.Equals(Vector3.negativeInfinity));
        }
    }

    bool Valid(float dist, Vector3 pos)
    {
        for (int i = 0; i < 4; i++)
        {
            if (Vector3.Distance(pos, transform.GetChild(i).position) < dist)
            {
                return false;
            }
        }

        foreach (GameObject go in _instantiated)
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

        _instantiated.Clear();

        foreach(Ship ship in SceneShips)
        {
            ship.ResetShip();
        }
    }
}
