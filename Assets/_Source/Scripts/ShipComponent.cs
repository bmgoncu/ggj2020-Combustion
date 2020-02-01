using System.Collections.Generic;
using UnityEngine;

/* 3 tip component  var */
public enum ShipComponentType { ORBITER, FUEL_TANK, ENGINE }

public class ShipComponent : MonoBehaviour
{
    public bool IsUsed { get; set; }
    /* component'in tipini inspector'dan veriyoruz */
    [SerializeField] ShipComponentType _type;
    SnapPoint[] _snapPoints;    // component'in snap noktaları bu dizide (awake'de alındı)

    void Awake()
    {
        _snapPoints = GetComponentsInChildren<SnapPoint>();
    }

    // her zaman component bir ship'e takılır
    public bool Snap(Ship ship)
    {
        if (ship.ShipComponents.Count == 0)
        {
            ship.AddShipComponent(this);
            transform.localPosition = Vector3.zero;
            IsUsed = true;
            return true;
        }

        /* burası çokomelli */
        /* ship üzerinde benim snap point'lerimi kabul eden snap point'ler listesi */
        List<SnapPoint>[] accepted = GetAcceptedSnapPoints(ship);

        /* EKLENECEK YER YOKSA COUNTER = 1 ve RETURN FALSE */
        int counter = 0;
        foreach(List<SnapPoint> element in accepted)
        {
            counter += element.Count;
        }
        if (counter == 0)
        {
            return false;
        }

        /* aşağısı problem çıkarır mı / bence çıkarmaz ama konuşalım */
        int index;
        do
        {
            index = Random.Range(0, _snapPoints.Length);
            if (accepted[index].Count == 0)
            {
                index = -1;
            }
        }
        while (index == -1);

        if (index != -1)
        {
            ship.AddShipComponent(this);

            SnapPoint selected = accepted[index][Random.Range(0, accepted[index].Count)];
            /* bizim snap point'imize selected'ı ekliyoruz ki Snap fonksiyonu aynı zamanda pozisyon ve rotasyon hesabını da yapsın */
            _snapPoints[index].Snap(selected, true);
            IsUsed = true;
            return true;
        }

        return false;
    }

    public SnapPoint[] GetSnapPoints() { return _snapPoints; }

    public ShipComponentType GetShipComponentType() { return _type; }

    #region IN-CLASS METHODS

    List<SnapPoint>[] GetAcceptedSnapPoints(Ship ship)
    {
        List<SnapPoint>[] list = new List<SnapPoint>[_snapPoints.Length];

        for (int i = 0; i < _snapPoints.Length; i++)    // her bir snap point'ime ...
        {
            list[i] = new List<SnapPoint>();            // ... bir liste

            foreach (ShipComponent sc in ship.ShipComponents)      // ship'teki her  component'in...
            {
                foreach (SnapPoint sp in sc.GetSnapPoints())            // ... snap point'ini kontrol et
                {
                    if (sp.IsAvailable(_type) && _snapPoints[i].IsAvailable(sc.GetShipComponentType()))
                    {
                        list[i].Add(sp);
                    }
                }
            }
        }

        return list;
    }

    #endregion
}
