using System.Collections;
using System.Collections.Generic;
using NDream.AirConsole;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    public Ship Ship;
    public List<ShipComponent> Components;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            Components[0].Snap(Ship);
        }
        if (Input.GetKeyUp(KeyCode.T))
        {
            Components[1].Snap(Ship);
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            Components[2].Snap(Ship);
        }


        if (Input.GetKeyUp(KeyCode.Q))
        {
            Components[3].Snap(Ship);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            FindObjectOfType<ControlManager>().StartGame(AirConsole.instance.GetControllerDeviceIds().Count);
        }
    }
}
