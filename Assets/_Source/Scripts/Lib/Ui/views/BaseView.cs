/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class BaseView : MonoBehaviour
{
    public Transform Root;

    public virtual void Awake()
    {
        if(Root == null)
        {
            Root = transform;
        }
    }
}
