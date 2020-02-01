using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] Text _text;

    float _timer = -1f;

    void Start()
    {
        
    }

    void Update()
    {
        if (_timer > 0f)
        {
            _timer -= Time.deltaTime;
            _text.text = (int)_timer + "";
            if (_timer < 0f)
            {
                ControlManager.Instance.Death();
            }
        }
    }

    public void CountDown(float duration)
    {
        _timer = duration;
    }
}
