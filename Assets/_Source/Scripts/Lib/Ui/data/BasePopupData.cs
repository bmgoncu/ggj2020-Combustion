/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */

using UnityEngine.Events;

public class BasePopupData
{
    protected string popupResourcePath = "UI/Popups/BasePopup";
    public string HeaderText;
    public string InnerText;
    public string AcceptButtonText;
    public string CancelButtonText;
    public UnityAction OnAcceptCallback;
    public UnityAction OnCancelCallback;

    public string GetResourcePath()
    {
        return popupResourcePath;
    }
}