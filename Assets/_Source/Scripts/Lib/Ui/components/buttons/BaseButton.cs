/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("UI/BaseButton")]
public class BaseButton : Button
{
    private Text textField;

    protected BaseButton()
    {
    }

    protected override void Awake()
    {
        base.Awake();
        if (gameObject.GetComponentInChildren<Text>())
        {
            textField = gameObject.GetComponentInChildren<Text>();
        }
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        PlayTapSound();
    }
    
    public void setText(string text)
    {
        if (textField)
        {
            textField.text = text;
        }
    }

    public void setText(string text, string textFieldName)
    {
        Transform _tfTransform = transform.Find(textFieldName);
        if (_tfTransform)
        {
            Text _tfComponent = _tfTransform.GetComponent<Text>();
            if (_tfComponent) _tfComponent.text = text;
        }
    }

    private void PlayTapSound()
    {
        //SoundManager.Instance.PlaySound(tapSound);
    }
}

