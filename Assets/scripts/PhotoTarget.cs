using UnityEngine;

public class PhotoTarget : MonoBehaviour
{
    public int basePoints = 100;
    public bool captured;
    
    public Vector3 GetAimPoint()
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers == null || renderers.Length == 0)
            return transform.position;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        return b.center;
    }
}
