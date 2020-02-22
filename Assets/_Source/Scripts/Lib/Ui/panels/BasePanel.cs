/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */

using System;

public interface IBasePanel {
    void Update();
    void SetData(BasePanelData data);
    void Show();
    void Hide();
    void SetVisible(bool isVisible);
    void OnDestroy();
}

public abstract class BasePanel<TV> : BaseController<TV>, IBasePanel where TV : BaseView {
    protected UIManager _manager;

    /// <summary>
    /// Creates the panel object and links its view to it.
    /// </summary>
    /// <typeparam name="TB">Panel Type</typeparam>
    /// <param name="manager">UiManager reference</param>
    /// <returns></returns>
    public static TB Create<TB>(UIManager manager) where TB : BasePanel<TV>
    {
        TB newPanel = (TB)Activator.CreateInstance(typeof(TB));
        newPanel._manager = manager;
        newPanel.OnInit();
        newPanel.isInitialized = true;

        return newPanel;
    }

    /// <summary>
    /// Updates the panels, called by the manager.
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Sets the data for the panel.
    /// </summary>
    /// <param name="data">BasePanelData of the panel.</param>
    public virtual void SetData(BasePanelData data) { }

    /// <summary>
    /// Shows the panel
    /// </summary>
    public void Show()
    {
        if (!IsInitialized)
        {
            OnInit();
            isInitialized = true;
        }

        OnShow();
    }

    /// <summary>
    /// Hides the panel.
    /// </summary>
    public void Hide()
    {
        OnHide();
    }

    /// <summary>
    /// Hides or Shows the panel.
    /// </summary>
    /// <param name="isVisible">Visibility</param>
    public void SetVisible(bool isVisible)
    {
        View.Root.gameObject.SetActive(isVisible);
    }

    protected abstract void OnInit();

    protected abstract void OnShow();

    protected abstract void OnHide();

    public virtual void OnDestroy() { }
}
