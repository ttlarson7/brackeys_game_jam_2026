using UnityEngine;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    public CinemachineCamera vcam;

    [Header("Zoom Settings")] 
    public float zoomspeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            float currentZoom = vcam.Lens.OrthographicSize;
            currentZoom -= scroll * zoomspeed;
            
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            
            vcam.Lens.OrthographicSize = currentZoom;
        }
    }
}
