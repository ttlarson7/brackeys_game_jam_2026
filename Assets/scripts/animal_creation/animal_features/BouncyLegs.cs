using UnityEngine;

public class SpringLeg : MonoBehaviour
{
    [Header("Spring Settings")]
    public float bounceSpeed = 3f;
    public float bounceAmount = 0.1f;
    public float squishAmount = 0.05f;

    private Vector3 originalPos;
    private Vector3 originalScale;

    void Start()
    {
        originalPos = transform.localPosition;
        originalScale = transform.localScale;
    }

    void Update()
    {
        float bounce = Mathf.Sin(Time.time * bounceSpeed) * bounceAmount;
        float squish = Mathf.Abs(Mathf.Sin(Time.time * bounceSpeed)) * squishAmount;

        transform.localPosition = originalPos + new Vector3(0, bounce, 0);
        transform.localScale = new Vector3(
            originalScale.x,
            originalScale.y + squish,
            originalScale.z
        );
    }
}