using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ship : MonoBehaviour
{
    /* ui which belongs to the ship */
    [SerializeField] ShipStatus _statusUI;
    /* explosion effect */
    [SerializeField] GameObject _explosionPrefab;

    public bool Escaped { get; private set; } = false;  /* true if the ship escaped */

    /* we assumed that a ship may contain more than 1 person */
    public int Capacity { get; private set; } = 0;
    /* actrually capacity is the number of orbiters attached */

    /* list of attached components */
    public List<ShipComponent> CurrentComponents { get; private set; } = new List<ShipComponent>();

    /* this method is called by the ship component sc */
    public void AddShipComponent(ShipComponent sc)
    {
        CurrentComponents.Add(sc);
        if (sc.Type == ShipComponentType.ORBITER)
        {
            Capacity++;
        }
        SetTotalChancePoint();
    }

    /* sometimes, we remove a random ShipComponent from the ship, if there's one */
    public ShipComponent RemoveShipComponent()
    {
        if (CurrentComponents.Count == 0)
        {
            return null;
        }

        int removeIndex = Random.Range(0, CurrentComponents.Count);
        ShipComponent removedPart = CurrentComponents[removeIndex];

        CurrentComponents.RemoveAt(removeIndex);

        SetTotalChancePoint();

        return removedPart;
    }

    public float SetTotalChancePoint()
    {
        float point = CurrentComponents.Sum(sc => sc.ChancePoint);
        _statusUI.SetValue(point);
        return point;
    }

    public void ResetShip()
    {
        /* 5% is to represent an empty status bar */
        _statusUI.SetValue(0.05f);

        Escaped = false;
        Capacity = 0;
        CurrentComponents.Clear();
    }

    /* following function will be updated after the implementation of the pooling manager */
    public void Explode() { Instantiate(_explosionPrefab, transform); }

    public void IncreaseCapacity() { Capacity++; }

    public void DecreaseCapacity() { Capacity--; }

    /* shows or hides the status ui attached to this ship */
    public void UpdateStatusUI(bool enabled) { _statusUI.gameObject.SetActive(enabled); }
}
