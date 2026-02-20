using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer mapRenderer;
    
    [Header("Movement")]
    public float moveSpeed = 10f;

    [Header("Padding")] 
    public float verticalPadding = 0f;

    private float leftX, rightX, minY, maxY, worldWidth;

    void Start()
    {
        if (!mapRenderer)
        {
            Debug.LogError("CameraRig: mapRenderer not assigned.");
            enabled = false;
            return;
        }
        
        Bounds b = mapRenderer.bounds;
        
        leftX = b.min.x;
        rightX = b.max.x;
        minY = b.min.y + verticalPadding;
        maxY = b.max.y + verticalPadding;
        
        worldWidth = rightX - leftX;
    }
    
    void Update()
    {
        float xIn = Input.GetAxisRaw("Horizontal");
        float yIn = Input.GetAxisRaw("Vertical");
        
        Vector3 p = transform.position;
        p.x += xIn * moveSpeed * Time.deltaTime;
        p.y += yIn * moveSpeed * Time.deltaTime;
        
        p.y = Mathf.Clamp(p.y, minY, maxY);

        float relX = p.x - leftX;
        relX = Mathf.Repeat(relX, worldWidth);
        p.x = leftX + relX;

        transform.position = p;
    }
}
