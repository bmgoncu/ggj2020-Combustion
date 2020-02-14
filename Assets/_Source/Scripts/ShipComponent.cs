using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/* there are three types of components */
public enum ShipComponentType { ORBITER, FUEL_TANK, ENGINE }

public class ShipComponent : MonoBehaviour
{
    readonly Vector3 EXTREMES = new Vector3(6f, -6f, 2f);

    [SerializeField] float _chancePoint;

    [SerializeField] ShipComponentType _type;

    public bool IsUsed { get; private set; }

    public float ChancePoint { get { return _chancePoint; } }

    public ShipComponentType Type { get { return _type; } }

    public SnapPoint[] SnapPoints { get; private set; }

    Sequence _cinematic;

    void Awake()
    {
        SnapPoints = GetComponentsInChildren<SnapPoint>();

        float firstDelay = Random.Range(0.3f, 0.5f);
        float duration = Random.Range(3f, 5f);
        float secondDelay = Random.Range(0.3f, 0.5f);

        _cinematic = DOTween.Sequence();
        _cinematic.AppendInterval(firstDelay);
        _cinematic.Append(transform.DOMoveY(1f, duration));
        _cinematic.Insert(firstDelay, transform.DORotate(transform.eulerAngles + 30f * Vector3.up, duration));
        _cinematic.AppendInterval(secondDelay);
        _cinematic.SetLoops(-1, LoopType.Yoyo);
    }

    void OnEnable()
    {
        Distraction.OnDistraction += OnDistraction;
    }

    void OnDisable()
    {
        Distraction.OnDistraction -= OnDistraction;
    }

    void OnDistraction(float magnitude, float direction)
    {
        if (!IsUsed)
        {
            float destX = transform.position.x;
            float destZ = transform.position.z;

            destX += magnitude * Mathf.Cos(direction);
            destZ += magnitude * Mathf.Sin(direction);

            destX = Mathf.Clamp(destX, -EXTREMES.x, EXTREMES.x);
            destZ = Mathf.Clamp(destZ, EXTREMES.y, EXTREMES.z);

            transform.DOMoveX(destX, 2f / magnitude);
            transform.DOMoveZ(destZ, 2f / magnitude);
        } 
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

        _cinematic.Pause();
    }

    public void Drop()
    {
        IsUsed = false;

        Vector3 pos = transform.position;
        pos.y = 0.2f;

        transform.SetParent(null);
        transform.position = pos;
        transform.localEulerAngles = Vector3.zero;

        _cinematic.Play();
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
