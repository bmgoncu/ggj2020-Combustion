using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    /* component types accepted by this snap point */
    [SerializeField] ShipComponentType[] _acceptedComponentTypes;

    /* adjacent snap point - null if not snapped yet */
    public SnapPoint Snapped { get; private set; }

    Transform _parent;

    void Awake()
    {
        /* the parent at the top always have a ShipComponent script */
        _parent = GetComponentInParent<ShipComponent>().transform;
    }

    /* if you send false as the second parameter, it only assigns Snapped value */
    /* when called for the first time make changeTransform = true */
    public void Snap(SnapPoint target, bool changeTransform)
    {
        Snapped = target;

        if (changeTransform)
        {
            target.Snap(this, false);

            _parent.SetParent(target.transform);

            _parent.localScale = Vector3.one;
            _parent.localPosition = Vector3.zero;
        }
    }

    /* same logic with the Snap function */
    public void Unsnap(bool changeParent)
    {
        if (changeParent)
        {
            Snapped.Unsnap(false);
            _parent.SetParent(null);
        }

        Snapped = null;
    }

    public bool IsAvailable(ShipComponentType type)
    {
        if (Snapped != null)
        {
            return false;
        }

        foreach (ShipComponentType sct in _acceptedComponentTypes)
        {
            if (sct == type)
            {
                return true;
            }
        }
        return false;
    }
}
