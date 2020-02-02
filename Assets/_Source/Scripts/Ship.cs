using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public int OwnerId;
    public ShipStatus StatusUI;
    public GameObject ExplosionPrefab;
    public List<ShipComponent> ShipComponents { get; private set; } = new List<ShipComponent>();
    public int Capacity;
    public bool Escaped;

    public void AddShipComponent(ShipComponent sc)
    {
        ShipComponents.Add(sc);
        sc.transform.SetParent(transform);
        if (sc.GetShipComponentType() == ShipComponentType.ORBITER)
            Capacity++;
        SetTotalChancePoint();
    }

    public ShipComponent RemoveShipComponent()
    {
        if (ShipComponents.Count == 0) return null;
        SetTotalChancePoint();
        var removeIndex = UnityEngine.Random.Range(0, ShipComponents.Count);
        var removedPart = ShipComponents[removeIndex];
        ShipComponents.RemoveAt(removeIndex);
        removedPart.transform.SetParent(transform);
        return removedPart;
    }

    public float SetTotalChancePoint()
    {
        var point = ShipComponents.Sum(sc => sc.ChancePoint);
        StatusUI.SetValue(point);
        return point;
    }

    public void ResetShip()
    {
        StatusUI.SetValue(0.05f);
        ShipComponents.Clear();
        Capacity = 0;
        Escaped = false;
    }

    public void Explode()
    {
        Instantiate(ExplosionPrefab, transform);
    }
}
