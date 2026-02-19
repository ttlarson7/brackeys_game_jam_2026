using UnityEngine;

public class PhotoScoring : MonoBehaviour
{
    [Header("References")] 
    public Camera mainCam;

    [Header("Scoring")] 
    public float maxCenterDistance = 0.35f;
    public AnimationCurve centerScoreCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Input")] public KeyCode shutterKey = KeyCode.Space;
    
    void Update()
    {
        if (Input.GetKey(shutterKey))
        {
            TakePhotoAndScore();
        }
    }

    void TakePhotoAndScore()
    {
        if (!mainCam)
        {
            Debug.LogError("No main camera");
            return;
        }
        
        PhotoTarget[] targets = FindObjectsByType<PhotoTarget>(FindObjectsSortMode.None);
        if (targets.Length == 0) return;

        Vector2 center = new Vector2(0.5f, 0.5f);

        PhotoTarget best = null;
        float bestDist = float.MaxValue;
        float bestNorm = 0f;

        foreach (var t in targets)
        {
            Vector3 vp = mainCam.WorldToViewportPoint(t.transform.position);

            if (vp.z <= 0f) continue;

            if (vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f) continue;

            float dist = Vector2.Distance(new Vector2(vp.x, vp.y), center);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = t;
            }
        }

        if (best == null)
        {
            Debug.Log("No target in frame.");
            return;
        }
        
        bestNorm = Mathf.Clamp01(bestDist / maxCenterDistance);
        
        float quality = 1f - centerScoreCurve.Evaluate(bestNorm);

        int basePts = best.basePoints;
        int score = Mathf.RoundToInt(basePts * Mathf.Clamp01(quality));

        Debug.Log($"Photo hit {best.name} | dist={bestDist:F3} | quality={quality:F2} | +{score}");
    }
}
