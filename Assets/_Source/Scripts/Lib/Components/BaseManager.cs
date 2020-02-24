/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */

using System;
using System.Collections;
using UnityEngine;


public abstract class BaseManager<T> : SingletonComponent<T> where T : MonoBehaviour
{
    public bool IsReady
    {
        get
        {
            return _isReady;
        }
    }

    private bool _isReady = false;
    private int _waitingAssetCount;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public abstract IEnumerator Init();
    public virtual void Destroy() { }
    public virtual void Load() { }
    public virtual void Save() { }
    
    public virtual void SetManagerReady()
    {
        _isReady = true;
    }
    
    public IEnumerator WaitForActivation(Action callback = null)
    {
        yield return new WaitUntil(() => _isReady);
        if (callback != null)
        {
            callback();
        }
    }
}
