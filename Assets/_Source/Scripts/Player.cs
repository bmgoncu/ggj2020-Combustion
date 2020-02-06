using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Ship Board;

    public int Id;
    public ShipComponent PickedComponent;
    public Rigidbody PhysicsRigidBody;
    public SkinnedMeshRenderer Renderer;

    public Transform Hand;

    Animator _animator;

    private const int INTERACTION_DIST = 1;
    private const float SHIP_DISTANCE = 3f;
    private const float VELOCITY_MULTIPLIER = 3;

    // Start is called before the first frame update
    void Start()
    {
        PhysicsRigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    public void SetColor(Color color)
    {
        Renderer.materials[1].color = color;
    }

    void Update()
    {
        if (Board != null)
        {
            return;
        }

        if (transform.position.x < -7.5f)
        {
            transform.position = new Vector3(-7.5f, 0f, transform.position.z);
            PhysicsRigidBody.velocity = new Vector3(0f, 0f, PhysicsRigidBody.velocity.z);
        }

        if (transform.position.x > 7.5f)
        {
            transform.position = new Vector3(7.5f, 0f, transform.position.z);
            PhysicsRigidBody.velocity = new Vector3(0f, 0f, PhysicsRigidBody.velocity.z);
        }

        if (transform.position.z < -7f)
        {
            transform.position = new Vector3(transform.position.x, 0f, -7f);
            PhysicsRigidBody.velocity = new Vector3(PhysicsRigidBody.velocity.x, 0f, 0f);
        }

        if (transform.position.z > 3f)
        {
            transform.position = new Vector3(transform.position.x, 0f, 3f);
            PhysicsRigidBody.velocity = new Vector3(PhysicsRigidBody.velocity.x, 0f, 0f);
        }
    }

    public void Move(Vector2 vec)
    {
        vec = vec.normalized;
        PhysicsRigidBody.velocity = new Vector3(vec.x * VELOCITY_MULTIPLIER, 0f, vec.y * VELOCITY_MULTIPLIER);

        float angle = Mathf.PI / 2f - Mathf.Atan2(vec.y, vec.x);
        transform.rotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
        
        if  (vec.Equals(Vector2.zero))
        {
            _animator.SetInteger("Param", PickedComponent == null ? 0 : 10);
        }
        else
        {
            _animator.SetInteger("Param", PickedComponent == null ? 1 : 11);
        }
    }

    public void DoAction()
    {
        if (Board)
        {
            return;
        }
        if (PickedComponent == null)
        {
            // Get advesary ships
            var availableShips = GetObjectInRange<Ship>(SHIP_DISTANCE, (s) => s.Escaped == false/*s.OwnerId != Id*/);
            var advesaryShip = (availableShips.Count > 0) ? availableShips[0] : null;
            var availableShipParts  = GetObjectInRange<ShipComponent>(INTERACTION_DIST, (sc) => !sc.IsUsed);
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
                foreach(Player player in FindObjectsOfType<Player>())
                {
                    if (player.Board == advesaryShip)
                    {
                        player.GetOut(advesaryShip);
                    }
                }
            }
            else if( selectedShipPart != null && !selectedShipPart.IsUsed)
            {
                Pick(selectedShipPart);
            }
        }
        else
        {
            // Get my ship
            var availableShips = GetObjectInRange<Ship>(INTERACTION_DIST, (s) => true/*s.OwnerId == Id*/);
            var ownerShip = (availableShips.Count > 0) ? availableShips[0] : null;

            if (ownerShip != null)
            {
                if(PickedComponent.Snap(ownerShip))
                {
                    PickedComponent = null;
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
        if (PickedComponent != null) return false;
        PickedComponent = component;
        component.transform.SetParent(Hand);
        component.transform.localPosition = Vector3.zero;
        component.transform.localEulerAngles = Vector3.zero;
        //component.transform.localPosition = transform.GetChild(0).localPosition;
        component.IsUsed = true;
        return true;
    }

    public ShipComponent Drop()
    {
        var comp = PickedComponent;
        if (comp == null) return null;
        comp.transform.SetParent(null);
        PickedComponent = null;
        comp.IsUsed = false;
        comp.transform.position = new Vector3(comp.transform.position.x, 0.2f, comp.transform.position.z);
        comp.transform.localEulerAngles = Vector3.zero;
        return comp;
    }

    private List<T> GetObjectInRange<T>(float range, Func<T,bool> condition) where T : MonoBehaviour
    {
        var emptyComponents = FindObjectsOfType<T>();
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
        if  (ship.Capacity > 0)
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
        PickedComponent = null;
    }
}
