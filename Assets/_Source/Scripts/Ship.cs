using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public int OwnerId;
    public List<ShipComponent> ShipComponents { get; private set; } = new List<ShipComponent>();

    public void AddShipComponent(ShipComponent sc)
    {
        ShipComponents.Add(sc);
        sc.transform.SetParent(transform);
    }

    public ShipComponent RemoveShipComponent()
    {
        if (ShipComponents.Count == 0) return null;

        var removeIndex = UnityEngine.Random.Range(0, ShipComponents.Count);
        var removedPart = ShipComponents[removeIndex];
        ShipComponents.RemoveAt(removeIndex);
        removedPart.transform.SetParent(transform);
        return removedPart;
    }
}
