using UnityEngine;

public class FlapAnimation : MonoBehaviour
{
    public float flapInterval = 0.1f;
    public float pauseInterval = 0.5f;
    public bool useSquish = false;      // Toggle in inspector
    public float flapScaleY = 0.05f;   // Only used when squishing

    private SpriteRenderer sr;
    private int flapCount = 0;
    private float timer = 0f;
    private bool isPausing = false;
    private bool isFlapped = false;
    private Vector3 originalScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isPausing)
        {
            if (timer >= pauseInterval)
            {
                isPausing = false;
                flapCount = 0;
                timer = 0f;
            }
        }
        else
        {
            if (timer >= flapInterval)
            {
                isFlapped = !isFlapped;

                if (useSquish)
                {
                    transform.localScale = isFlapped
                        ? new Vector3(originalScale.x, flapScaleY, originalScale.z)
                        : originalScale;
                }
                else
                {
                    sr.flipY = !sr.flipY;
                }

                flapCount++;
                timer = 0f;

                if (flapCount >= 6)
                    isPausing = true;
            }
        }
    }
}