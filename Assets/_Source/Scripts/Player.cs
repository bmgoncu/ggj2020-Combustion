using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int Id;
    public ShipComponent PickedComponent;
    public Rigidbody PhysicsRigidBody;

    private const int INTERACTION_DIST = 3;

    // Start is called before the first frame update
    void Start()
    {
        PhysicsRigidBody = GetComponent<Rigidbody>();
    }

    public void Move(Vector2 vec)
    {
        PhysicsRigidBody.velocity = new Vector3(vec.x, 0f, vec.y);
        float angle = Mathf.PI - Mathf.Atan2(vec.y, vec.x);
        transform.rotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
    }

    public void DoAction()
    {
        if (PickedComponent == null)
        {
            // Get advesary ships
            var availableShips = GetObjectInRange<Ship>(INTERACTION_DIST, (s) => true/*s.OwnerId != Id*/);
            var advesaryShip = (availableShips.Count > 0) ? availableShips[0] : null;
            var availableShipParts  = GetObjectInRange<ShipComponent>(INTERACTION_DIST, (sc) => !sc.IsUsed);
            var selectedShipPart = (availableShipParts.Count > 0) ? availableShipParts[UnityEngine.Random.Range(0, availableShipParts.Count)] : null;

            if (advesaryShip != null && advesaryShip.transform.childCount > 0)
            {
                var stolenPart = advesaryShip.RemoveShipComponent();
                Pick(stolenPart);
            }
            else if( selectedShipPart != null)
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
                PickedComponent.Snap(ownerShip);
                PickedComponent = null;
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
        component.transform.SetParent(transform);
        component.transform.localPosition = transform.GetChild(0).localPosition;
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
        comp.transform.position = new Vector3(comp.transform.position.x, 0f, comp.transform.position.z);
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
}
