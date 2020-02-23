using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;

public class LobbyPanel : BasePanel<LobbyView>
{
    private LobbyPanelData _data;
    
    protected override void OnInit()
    {
        View = SceneResourceManager.Instance.GetObject("LobbyPanel").GetComponent<LobbyView>();
        
    }
    
    private void OnHomeClicked()
    {
        //throw new NotImplementedException();
    }

    public override void SetData(BasePanelData data)
    {
        _data = data as LobbyPanelData;
    }
    
    protected override void OnHide()
    {
    }
    
    protected override void OnShow()
    {
    }
}
