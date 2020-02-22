using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class BaseController<T> where T : BaseView 
{
    public T View;
    protected bool isInitialized = false;

    public virtual bool isVisible { get { return IsInitialized && View.Root.gameObject.activeSelf; } }
    public bool IsInitialized { get { return isInitialized; } }

    public virtual void Init(T view)
    {
        this.View = view;
    }
}
