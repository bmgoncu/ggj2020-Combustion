using System.Collections.Generic;
using UnityEngine;

/* there are three types of components */
public enum ShipComponentType { ORBITER, FUEL_TANK, ENGINE }

public class ShipComponent : MonoBehaviour
{
    [SerializeField] float _chancePoint;

    [SerializeField] ShipComponentType _type;

    public bool IsUsed { get; private set; }

    public float ChancePoint { get { return _chancePoint; } }

    public ShipComponentType Type { get { return _type; } }

    public SnapPoint[] SnapPoints { get; private set; }

    void Awake()
    {
        SnapPoints = GetComponentsInChildren<SnapPoint>();
    }

    /* components are always attached to a ship */
    public bool Attach(Ship ship)
    {
        /* when this component is the first component attached to the ship */
        if (ship.CurrentComponents.Count == 0)
        {
            IsUsed = true;

            ship.AddShipComponent(this);

            transform.SetParent(ship.transform);
            transform.localPosition = Vector3.up * 0.2f;
            transform.localRotation = Quaternion.identity;

            return true;
        }

        /* after this point, we're looking for an available snap point */

        List<SnapPoint>[] accepted = GetAcceptedSnapPoints(ship);

        int counter = 0;
        foreach(List<SnapPoint> element in accepted)
        {
            counter += element.Count;
        }

        if (counter == 0)
        {
            return false;
        }

        int index;
        do
        {
            index = Random.Range(0, SnapPoints.Length);
        }
        while (accepted[index].Count == 0);

        if (index != -1)
        {
            IsUsed = true;

            ship.AddShipComponent(this);

            transform.SetParent(ship.transform);

            SnapPoints[index].Snap(accepted[index][Random.Range(0, accepted[index].Count)], true);

            return true;
        }

        return false;
    }

    public void Pick(Transform hand)
    {
        foreach (SnapPoint sp in SnapPoints)
        {
            sp.Unsnap(true);
        }

        IsUsed = true;

        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
    }

    public void Drop()
    {
        IsUsed = false;

        Vector3 pos = transform.position;
        pos.y = 0.2f;

        transform.SetParent(null);
        transform.position = pos;
        transform.localEulerAngles = Vector3.zero;
    }

    List<SnapPoint>[] GetAcceptedSnapPoints(Ship ship)
    {
        List<SnapPoint>[] list = new List<SnapPoint>[SnapPoints.Length];

        for (int i = 0; i < SnapPoints.Length; i++)
        {
            list[i] = new List<SnapPoint>();

            foreach (ShipComponent sc in ship.CurrentComponents)
            {
                foreach (SnapPoint sp in sc.SnapPoints)
                {
                    if (sp.IsAvailable(_type) && SnapPoints[i].IsAvailable(sc.Type))
                    {
                        list[i].Add(sp);
                    }
                }
            }
        }

        return list;
    }
}
