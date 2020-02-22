/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BasePopup : MonoBehaviour
{
    private BasePopupData _data;
    private BasePopupView _view;

    public virtual void SetData(BasePopupData data)
    {
        _data = data;
    }

    public void Show()
    {
        Init();
        OnShow();
    }

    public void Hide()
    {
        OnHide();
        GameObject.Destroy(this.gameObject);
    }

    protected virtual void OnShow() { }

    protected virtual void OnHide() { }

    protected void Init()
    {
        _view = transform.GetComponent<BasePopupView>();
        _view.HeaderText.text = _data.HeaderText;
        _view.InnerText.text = _data.InnerText;
        _view.AcceptButton.setText(_data.AcceptButtonText);
        _view.CancelButton.setText(_data.CancelButtonText);

        _view.AcceptButton.onClick.AddListener(_data.OnAcceptCallback);

        _view.CancelButton.onClick.AddListener(_data.OnCancelCallback);
    }

}