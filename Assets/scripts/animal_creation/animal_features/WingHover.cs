using UnityEngine;

public class WingHover : MonoBehaviour
{
    public float amplitude = 0.15f;
    public float frequency = 1.5f;

    private Rigidbody2D _rb;
    private float _timeOffset;

    private void Start()
    {
        _rb = GetComponentInParent<Rigidbody2D>();
        _timeOffset = Random.Range(0f, Mathf.PI * 2f);
        
        if (_rb == null)
            Debug.LogWarning("WingHover: No Rigidbody2D found on parent!", this);
        else
            Debug.Log("WingHover attached to: " + _rb.gameObject.name);
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;
        if (_rb != null)
            _rb.gravityScale = 0f;
        float targetVY = Mathf.Cos((Time.time * frequency) + _timeOffset) * amplitude;
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, targetVY);
    }
}