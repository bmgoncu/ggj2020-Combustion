using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;

public class TitleScreenPanel : BasePanel<TitleScreenView>
{
    private TitleScreenPanelData _data;
    
    protected override void OnInit()
    {
        View = SceneResourceManager.Instance.GetObject("TitleScreenPanel").GetComponent<TitleScreenView>();
        
    }
    
    private void OnHomeClicked()
    {
        //throw new NotImplementedException();
    }

    public override void SetData(BasePanelData data)
    {
        _data = data as TitleScreenPanelData;
    }
    
    protected override void OnHide()
    {
    }
    
    protected override void OnShow()
    {
    }
}
