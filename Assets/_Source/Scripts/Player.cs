using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /* TODO: following values should be calibrated */
    const float _INTERACTION_DIST_ = 1f;
    const float _SHIP_DISTANCE_ = 3f;
    const float _VELOCITY_MULTIPLIER_ = 5f;

    [SerializeField] Transform _hand;

    [SerializeField] SkinnedMeshRenderer _renderer;

    public int Id { get; private set; }
    public int Index { get; private set; }

    public Ship Board { get; private set; }

    Animator _animator;
    ShipComponent _pickedComponent;
    Rigidbody _physicsBody;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _physicsBody = GetComponent<Rigidbody>();
    }

    public void Initialize(int id)
    {
        Id = id;
    }

    /* TODO: let's use better looking colors below */
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

    /*
     * TODO: GetObjectInRange fonksiyonu değiştirilmeli mi?
     * 1 - liste almaya gerek yok, bana en yakın lazım
     * 2 - liste yakınlığa göre sıralı değil. İlk eleman daha uzakta olsa da range içerisindeyse alıyor.
     * Ayrıca gemiye binme kontrolü vs burada olmalı gibi geliyor bana.
     */
    public void DoAction()
    {
        if (Board)
        {
            return;
        }
        if (_pickedComponent == null)
        {
            // Get advesary ships
            var availableShips = GetObjectInRange<Ship>(_SHIP_DISTANCE_, (s) => s.Escaped == false/*s.OwnerId != Id*/);
            var advesaryShip = (availableShips.Count > 0) ? availableShips[0] : null;
            var availableShipParts = GetObjectInRange<ShipComponent>(_INTERACTION_DIST_, (sc) => !sc.IsUsed);
            var selectedShipPart = (availableShipParts.Count > 0) ? availableShipParts[UnityEngine.Random.Range(0, availableShipParts.Count)] : null;

            bool shipiscloser = true;
            if (selectedShipPart && advesaryShip)
            {
                shipiscloser = Vector3.Distance(transform.position, advesaryShip.transform.position) < Vector3.Distance(transform.position, selectedShipPart.transform.position);
            }

            if (advesaryShip != null && advesaryShip.transform.childCount > 0 && shipiscloser)
            {
                var stolenPart = advesaryShip.RemoveShipComponent();
                Pick(stolenPart);
                foreach (Player player in FindObjectsOfType<Player>())
                {
                    if (player.Board == advesaryShip)
                    {
                        player.GetOut(advesaryShip);
                    }
                }
            }
            else if (selectedShipPart != null && !selectedShipPart.IsUsed)
            {
                Pick(selectedShipPart);
            }
        }
        else
        {
            // Get my ship
            var availableShips = GetObjectInRange<Ship>(_INTERACTION_DIST_, (s) => true/*s.OwnerId == Id*/);
            var ownerShip = (availableShips.Count > 0) ? availableShips[0] : null;

            if (ownerShip != null)
            {
                if (_pickedComponent.Attach(ownerShip))
                {
                    _pickedComponent = null;
                }
                /*
                
                */
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

    private List<T> GetObjectInRange<T>(float range, Func<T, bool> condition) where T : MonoBehaviour
    {
        var emptyComponents = FindObjectsOfType<T>(); // <-- we eliminated?
        List<T> results = new List<T>();
        for (int i = 0; i < emptyComponents.Length; i++)
        {
            var comp = emptyComponents[i];
            if (condition(comp) && Vector3.Distance(comp.transform.position, transform.position) < range)
            {
                results.Add(comp);
            }
        }
        return results;
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
}
