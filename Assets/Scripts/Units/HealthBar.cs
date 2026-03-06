using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fill;
    public Vector3 worldOffset = new Vector3(0, 1.2f, 0);

    public float followSmooth = 20f;
    public float damageLagSpeed = 3f;
    public float bumpHeight = 0.15f;
    public float bumpReturnSpeed = 10f;

    private Transform _target;
    private Camera _cam;

    private float _current01 = 1f;
    private float _display01 = 1f;

    private float _bump;
    private Vector3 _pos;

    public void Init(Transform target, Camera cam)
    {
        _target = target;
        _cam = cam;
        _pos = transform.position;
    }

    public void Set01(float t)
    {
        t = Mathf.Clamp01(t);

        if (t < _current01)
            _bump = bumpHeight;

        _current01 = t;
    }

    private void LateUpdate()
    {
        if (_target == null) { Destroy(gameObject); return; }
        if (_cam == null) _cam = Camera.main;

        _display01 = Mathf.MoveTowards(_display01, _current01, damageLagSpeed * Time.deltaTime);

        if (fill != null) fill.fillAmount = _display01;

        _bump = Mathf.MoveTowards(_bump, 0f, bumpReturnSpeed * Time.deltaTime);

        Vector3 targetPos = _target.position + worldOffset + Vector3.up * _bump;
        _pos = Vector3.Lerp(_pos, targetPos, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
        transform.position = _pos;

        if (_cam != null)
        {
            transform.rotation = _cam.transform.rotation;
        }
    }
}