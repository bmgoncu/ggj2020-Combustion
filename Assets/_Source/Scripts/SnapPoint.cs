using UnityEngine;

/* TODO: Konuşmamız gereken şeyler var design üzerine*/
public class SnapPoint : MonoBehaviour
{
    /* component types accepted by this snap point */
    [SerializeField] ShipComponentType[] _acceptedComponentTypes;

    /* adjacent snap point - null if not snapped yet */
    public SnapPoint Snapped { get; private set; }

    Transform _parent;

    Vector3 _offset;

    void Awake()
    {
        /* the parent at the top always have a ShipComponent script */
        _parent = GetComponentInParent<ShipComponent>().transform;

        _offset = transform.localPosition;

        /* offset with respect to the top parent */
        Transform traverse = transform.parent;
        while (traverse != _parent)
        {
            _offset += traverse.localPosition;
            traverse = traverse.parent;
        }

        _offset *= 3f;
    }

    /* if you send false as the second parameter, it only assigns Snapped value */
    /* when called for the first time make changeTransform = true */
    public void Snap(SnapPoint target, bool changeTransform)
    {
        Snapped = target;

        if (changeTransform)
        {
            target.Snap(this, false);

            Debug.Log("OFFSET : " + _offset);

            _parent.position = target.transform.position - _offset;
            _parent.eulerAngles = target.transform.eulerAngles;
        }
    }

    /* same logic with the Snap function */
    public void Unsnap(bool changeParent)
    {
        if (changeParent && Snapped)
        {
            Snapped.Unsnap(false);
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
