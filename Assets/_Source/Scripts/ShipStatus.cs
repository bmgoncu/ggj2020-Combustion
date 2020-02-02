using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipStatus : MonoBehaviour
{
    public Slider ChanceSlider;
    public Image Bar;

    // Start is called before the first frame update

    public void SetValue(float value)
    {
        ChanceSlider.value = value;

        if(value  < 0.33f)
        {
            Bar.color = Color.red;
        }else if(value < 0.66f)
        { 
            Bar.color = Color.yellow;
        }
        else
        {
            Bar.color = Color.green;
        }
    }
}
