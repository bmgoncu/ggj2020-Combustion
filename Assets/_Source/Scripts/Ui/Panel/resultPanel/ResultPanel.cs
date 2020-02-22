using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;

public class ResultPanel : BasePanel<ResultView>
{
    private ResultPanelData _data;
    private Coroutine _countdownRoutine;
    
    protected override void OnInit()
    {
        View = SceneResourceManager.Instance.GetObject("ResultPanel").GetComponent<ResultView>();
        
    }
    
    private void OnHomeClicked()
    {
        //throw new NotImplementedException();
    }

    public override void SetData(BasePanelData data)
    {
        _data = data as ResultPanelData;
    }
    
    protected override void OnHide()
    {
    }
    
    protected override void OnShow()
    {
        View.Title.text = (_data.IsWin) ? "You Win" : "You Lose";
        View.CountDown.text = (_data.CountDown--).ToString();
        
        for (int i = 0; i< View.Players.Count; i++)
        {
            View.Players[i].SetActive((i < _data.VictoriousPlayers.Count));
        }
        
        _countdownRoutine = AsyncUtils.Instance.SetRepeatingAction(() =>
            {
                View.CountDown.text = (_data.CountDown--).ToString();
                if (_data.CountDown < 0)
                {
                    AsyncUtils.Instance.StopCoroutine(_countdownRoutine);
                    UIManager.Instance.HidePanel(UIPanelType.Result);
                }
            }
            , 1f, 0);
    }
}
