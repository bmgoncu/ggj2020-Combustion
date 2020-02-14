using DG.Tweening;
using UnityEngine;

// TODO: we should polish explosions
public class Distraction : SingletonComponent<Distraction>
{
    [SerializeField] float _minPeriod;
    [SerializeField] float _maxPeriod;

    [SerializeField] float _minMagnitude;
    [SerializeField] float _maxMagnitude;

    bool _distracting;

    float _timer;
    float _period;

    /* ShipComponents are registered to this event */
    public delegate void DistractionEvent(float magnitude, float direction);
    public static DistractionEvent OnDistraction;

    void Start()
    {
        AssignPeriod();
    }

    void Update()
    {
        if (_distracting)
        {
            if ((_timer += Time.deltaTime) > _period)
            {
                _timer = 0f;

                float magnitude = Random.Range(_minMagnitude, _maxMagnitude);
                ScreenShake(magnitude);
                OnDistraction?.Invoke(magnitude, Random.Range(0f, 2f * Mathf.PI));
            }
        }
        
    }

    void AssignPeriod() { _period = Random.Range(_minPeriod, _maxPeriod); }

    void ScreenShake(float magnitude)
    {
        Camera.main.transform.DOShakePosition(2f, magnitude /  3f, 10, 90f, false, true);
    }

    public void EnableDistraction() { _distracting = true; }

    public void DisableDistraction() { _distracting = false; }
}
