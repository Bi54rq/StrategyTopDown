using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fill;
    public Vector3 worldOffset = new Vector3(0, 1.2f, 0);
    private Transform _target;
    private Camera _cam;

    public void Init(Transform target, Camera cam)
    {
        _target = target;
        _cam = cam;
    }

    public void Set01(float t)
    {
        t = Mathf.Clamp01(t);
        if (fill != null) fill.fillAmount = t;
    }

    private void LateUpdate()
    {
        if (_target == null) { Destroy(gameObject); return; }
        transform.position = _target.position + worldOffset;

        if (_cam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
        }
    }
}