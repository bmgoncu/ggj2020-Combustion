using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class PopupManager
{
    private static List<BasePopup> _displayedPopups = new List<BasePopup>();

    /// <summary>
    /// Shows popup with data.
    /// </summary>
    /// <param name="data">Data</param>
    /// <returns>BasePopup</returns>
    public static BasePopup Show(BasePopupData data)
    {
        BasePopup popup = Utils.InstantiatePrefab(data.GetResourcePath(), Vector3.zero, SceneResourceManager.Instance.GetObject("UI Root").transform).GetComponent<BasePopup>();
        popup.SetData(data);
        popup.Show();
        _displayedPopups.Add(popup);
        return popup;
    }

    /// <summary>
    /// Hides the Popup
    /// </summary>
    /// <param name="basePopup">Popup</param>
    public static void Hide(BasePopup basePopup)
    {
        _displayedPopups.Remove(basePopup);
        basePopup.Hide();
    }

    /// <summary>
    /// Hides the currently visible popup.
    /// </summary>
    public static void HideForeMost()
    {
        BasePopup popup = _displayedPopups.Last();
        Hide(popup);
    }
}
