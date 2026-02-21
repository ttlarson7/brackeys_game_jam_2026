using UnityEngine;

public class FlapAnimation : MonoBehaviour
{
    public float flapInterval = 0.1f;
    public float pauseInterval = 0.5f;
    public bool useSquish = false;
    public float flapScaleY = 0.05f;
    public SpriteRenderer wing;
    private int flapCount = 0;
    private float timer = 0f;
    private bool isPausing = false;
    private bool isFlapped = false;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        
        wing.material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
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
                    transform.localEulerAngles = isFlapped
                        ? new Vector3(0f, 0f, -90f)
                        : new Vector3(0,0,0f);
                }

                flapCount++;
                timer = 0f;

                if (flapCount >= 6)
                    isPausing = true;
            }
        }
    }
}