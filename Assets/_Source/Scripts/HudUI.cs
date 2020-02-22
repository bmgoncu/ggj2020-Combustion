using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HudUI : MonoBehaviour
{
    [SerializeField] Text _countdown;
    [SerializeField] Text _timer;

    float _counter = -1f;

    void Update()
    {
        if (_counter > 0f)
        {
            _counter -= Time.deltaTime;
            _timer.text = (int)_counter + "";
            if (_counter < 0f)
            {
                ControlManager.Instance.Death();
                _timer.gameObject.SetActive(false);
            }
        }
    }

    public void StartCountdown(int duration)
    {
        StartCoroutine(Countdown(duration));
    }

    public void StartTimer(float duration)
    {
        _timer.gameObject.SetActive(true);
        _counter = duration;
    }

    IEnumerator Countdown(int duration)
    {
        _countdown.gameObject.SetActive(true);
        for (int i = duration; i > 0; i--)
        {
            _countdown.text = i + "";
            yield return new WaitForSeconds(1f);
        }
        ControlManager.Instance.StartGame();
        _countdown.gameObject.SetActive(false);
    }
}
