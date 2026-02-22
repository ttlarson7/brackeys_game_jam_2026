using UnityEngine;
using System.Collections;

public class IntroOverlayController : MonoBehaviour
{
    public CanvasGroup group;

    [Header("Fade")] 
    public float fadeOutTime = 0.25f;

    public MonoBehaviour[] scriptsToDisable;
    
    bool dismissed;

    void Awake()
    {
        if (group != null)
            {
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
            }
        
        foreach (var s in scriptsToDisable)
            if (s != null) s.enabled = false;
    }
    void Update()
    {
        if (dismissed) return;
        
        if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
            {
            dismissed = true;
            StartCoroutine(FadeOutAndEnableGameplay());
            }
    }
    
    IEnumerator FadeOutAndEnableGameplay()
    {
        float t = 0f;
        float start = group != null ? group.alpha : 1f;

        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(start, 0f, t / fadeOutTime);
            if (group != null) group.alpha = a;
            yield return null;
        }

        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
        
        foreach (var s in scriptsToDisable)
            if (s != null) s.enabled = true;
        
        gameObject.SetActive(false);
    }
}
