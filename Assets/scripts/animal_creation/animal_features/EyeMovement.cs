using UnityEngine;

public class EyeFollow : MonoBehaviour
{
    public float pupilRadius = 0.15f;
    public Transform pupil;

    private Camera mainCamera;

    void Start()
    {
        // Finds whichever camera has the MainCamera tag
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null || pupil == null) return;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = mainCamera.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);

        Vector2 direction = (mouseWorld - transform.position).normalized;
        pupil.localPosition = new Vector3(direction.x * pupilRadius, direction.y * pupilRadius, pupil.localPosition.z);
    }
}