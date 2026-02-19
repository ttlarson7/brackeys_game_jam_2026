using UnityEngine;
using System.Collections;

public class CameraShutterUI : MonoBehaviour
{
	public CanvasGroup flashGroup;
	public float flashAlpha = 1f;
	public float flashDuration = 0.08f;

	void Start()
	{
		if (flashGroup == null) return;
				flashGroup.alpha = 0f;
				flashGroup.transform.SetAsLastSibling();
	}
	
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
	        StartCoroutine(Flash());
        }
        Debug.Log("Pressed space");
    }

    IEnumerator Flash()
    {
	    flashGroup.alpha = flashAlpha;
	    yield return new WaitForSeconds(flashDuration);
	    flashGroup.alpha = 0f;
    }
}
