using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    const float _SHIP_INTERACTION_DISTANCE_ = 1f;
    const float _VELOCITY_MULTIPLIER_ = 5f;

    [SerializeField] Transform _hand;

    [SerializeField] SkinnedMeshRenderer _renderer;

    public int Id { get; private set; }
    public int Index { get; private set; }

    public Ship Board { get; private set; }

    public ShipComponent InRange { get; private set; }

    Animator _animator;
    ShipComponent _pickedComponent;
    Rigidbody _physicsBody;

    public delegate void ShipComponentApproachedEvent(ShipComponent component);
    public static ShipComponentApproachedEvent OnShipComponentApproached;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _physicsBody = GetComponent<Rigidbody>();
    }

    public void Initialize(int id)
    {
        Id = id;
    }

    public void Initialize(int id, int index)
    {
        Initialize(id);

        Index = index;

        switch (index)
        {
            case 0:
                _renderer.materials[1].color = Color.red;
                break;
            case 1:
                _renderer.materials[1].color = Color.green;
                break;
            case 2:
                _renderer.materials[1].color = Color.blue;
                break;
            case 3:
                _renderer.materials[1].color = Color.magenta;
                break;
        }
    }

    public string GetColor()
    {
        Color c = _renderer.materials[1].color;

        if (c.Equals(Color.red)) { return "red"; }
        if (c.Equals(Color.green)) { return "green"; }
        if (c.Equals(Color.blue)) { return "blue"; }
        if (c.Equals(Color.magenta)) { return "purple"; }

        return "gray";
    }

    public void Move(Vector2 vec)
    {
        vec = vec.normalized;
        _physicsBody.velocity = _VELOCITY_MULTIPLIER_ * new Vector3(vec.x, 0f, vec.y);

        if (!vec.Equals(Vector2.zero))
        {
            float angle = Mathf.PI / 2f - Mathf.Atan2(vec.y, vec.x);
            transform.rotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
        }

        if (vec.Equals(Vector2.zero))
        {
            _animator.SetInteger("Param", _pickedComponent == null ? 0 : 10);
        }
        else
        {
            _animator.SetInteger("Param", _pickedComponent == null ? 1 : 11);
        }
    }

    public void DoAction()
    {
        if (Board)
        {
            return;
        }

        float dist = float.MaxValue;
        Ship ship = null;
        foreach (Ship sceneShip in StageManager.Instance.SceneShips)
        {
            float currDist = Vector3.Distance(sceneShip.transform.position, transform.position);
            if (currDist < dist && currDist < _SHIP_INTERACTION_DISTANCE_ && !sceneShip.Escaped)
            {
                dist = currDist;
                ship = sceneShip;
            }
        }

        if (_pickedComponent == null)
        {
            bool shipiscloser = true;

            if (InRange && ship)
            {
                shipiscloser = Vector3.Distance(transform.position, ship.transform.position) < Vector3.Distance(transform.position, InRange.transform.position);
            }

            if (ship && ship.transform.childCount > 0 && shipiscloser)
            {
                var stolenPart = ship.RemoveShipComponent();
                Pick(stolenPart);
                foreach (Player player in FindObjectsOfType<Player>())
                {
                    if (player.Board == ship)
                    {
                        player.GetOut(ship);
                    }
                }
            }
            else if (InRange != null)
            {
                Pick(InRange);
                InRange = null;
                OnShipComponentApproached?.Invoke(null);
            }
        }
        else
        {
            if (ship != null)
            {
                if (_pickedComponent.Attach(ship))
                {
                    _pickedComponent = null;
                }
            }
            else
            {
                Drop();
            }
        }
    }

    public bool Pick(ShipComponent component)
    {
        if (_pickedComponent != null)
        {
            return false;
        }

        _pickedComponent = component;

        component.Pick(_hand);

        return true;
    }

    public ShipComponent Drop()
    {
        ShipComponent comp = _pickedComponent;

        if (comp == null)
        {
            return null;
        }

        comp.Drop();

        _pickedComponent = null;

        return comp;
    }

    public void OnBoard(Ship ship)
    {
        if (ship.Capacity > 0)
        {
            transform.SetParent(ship.transform);
            Board = ship;
            transform.position = ship.transform.position;
            ship.DecreaseCapacity();
        }
    }

    public void GetOut(Ship ship)
    {
        transform.SetParent(null);
        ship.IncreaseCapacity();
        transform.position -= transform.position.normalized;
        Board = null;
    }

    public void ResetPlayer()
    {
        transform.SetParent(null);

        Board = null;

        _pickedComponent = null;

        _physicsBody.velocity = Vector2.zero;

        _animator.SetInteger("Param", 0);
    }

    void OnTriggerEnter(Collider other)
    {
        InRange = other.GetComponent<ShipComponent>();
        if (!InRange || InRange.IsUsed)
        {
            InRange = null;
            return;
        }
        OnShipComponentApproached?.Invoke(InRange);
    }

    void OnTriggerExit(Collider other)
    {
        if (InRange)
        {
            InRange = null;
            OnShipComponentApproached?.Invoke(null);
        }
    }
}
