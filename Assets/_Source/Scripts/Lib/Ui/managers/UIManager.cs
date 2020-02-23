/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class UIManager : BaseManager<UIManager>
{
    private Dictionary<UIPanelType, IBasePanel> _uiPanels;


    /// <summary>
    /// Creates and initializes all panels.
    /// </summary>
    public override IEnumerator Init() {
        _uiPanels = new Dictionary<UIPanelType, IBasePanel>();
        CreatePanels();

        SetManagerReady();
        yield return null;
    }

    private void CreatePanels()
    {
        //_uiPanels.Add(UIPanelType.Hud, BasePanel<HudView>.Create<HudPanel>(this));
        _uiPanels.Add(UIPanelType.Result, BasePanel<ResultView>.Create<ResultPanel>(this));
        _uiPanels.Add(UIPanelType.Lobby, BasePanel<LobbyView>.Create<LobbyPanel>(this));
    }

    /// <summary>
    /// Shows or hides panel
    /// </summary>
    /// <param name="type">Panel type</param>
    /// <param name="enable">Visibility</param>
    /// <param name="data">Panel Data</param>
    /// <returns>IBasePanel</returns>
    public IBasePanel ActivatePanel(UIPanelType type, bool enable = true, BasePanelData data = null)
    {
        if (enable) {
            return ShowPanel(type, data);
        } else {
            return HidePanel(type);
        }
    }

    /// <summary>
    /// Shows the panel
    /// </summary>
    /// <param name="type">Panel type</param>
    /// <param name="data">Panel Data</param>
    /// <returns>IBasePanel</returns>
    public IBasePanel ShowPanel(UIPanelType type, BasePanelData data)
    {
        IBasePanel ui = GetPanel(type);
        if(data != null) ui.SetData(data);
        ui.Show();
        ui.SetVisible(true);

        return ui;
    }
    
    /// <summary>
    /// Hides the panel
    /// </summary>
    /// <param name="type">Panel type</param>
    /// <returns>IBasePanel</returns>
    public IBasePanel HidePanel(UIPanelType type)
    {
        IBasePanel ui = GetPanel(type);
        ui.SetVisible(false);
        ui.Hide();

        return ui;
    }

    private IBasePanel GetPanel(UIPanelType type)
    {
        return _uiPanels[type];
    }

}

public enum UIPanelType
{
    Hud,
    Result,
    Lobby
}