using System.Collections.Generic;
using UnityEngine;

public class ShapeChainSpawner : MonoBehaviour
{
    [Header("Chain Settings")]
    public Vector2 spawnOrigin = Vector2.zero;
    public float shapeSize = 0.4f;
    private float spacing;

    [Header("Physics")]
    public bool anchorFirstShape = false;

    private List<GameObject> _chain = new List<GameObject>();
    private GameObject _dragTarget;
    private Vector2 _dragOffset;
    private Camera _cam;

    private enum ShapeType { Circle, Square, Triangle }

    private void Start()
    {
        _cam = Camera.main;
        spacing = 2f * shapeSize;
        SpawnChain();
    }

    private void Update()
    {
        HandleMouseDrag();
    }

    public void Respawn()
    {
        DestroyChain();
        SpawnChain();
    }

    private void SpawnChain()
    {
        int count = Random.Range(1, 6);

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = spawnOrigin + Vector2.right * (i * spacing);
            ShapeType type = (ShapeType)Random.Range(0, 3);
            Color color = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f);

            GameObject shape = CreateShape(type, color, pos);
            shape.transform.position = new Vector3(pos.x, pos.y, 1);
            _chain.Add(shape);

            if (i > 0)
                ConnectShapes(_chain[i - 1], shape);
        }

        if (anchorFirstShape && _chain.Count > 0)
            _chain[0].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    }

    private void DestroyChain()
    {
        foreach (var go in _chain)
            if (go != null) Destroy(go);
        _chain.Clear();
    }

    private GameObject CreateShape(ShapeType type, Color color, Vector2 position)
    {
        switch (type)
        {
            case ShapeType.Circle:  return CreateCircle(color, position);
            case ShapeType.Square:  return CreateSquare(color, position);
            default:                return CreateTriangle(color, position);
        }
    }

    private GameObject CreateCircle(Color color, Vector2 pos)
    {
        var go = new GameObject("Circle");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = color;
        sr.transform.localScale = Vector3.one * shapeSize * 2f;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        AddRigidbody(go);

        return go;
    }

    private GameObject CreateSquare(Color color, Vector2 pos)
    {
        var go = new GameObject("Square");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        sr.transform.localScale = Vector3.one * shapeSize * 2f;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        AddRigidbody(go);

        return go;
    }

    private GameObject CreateTriangle(Color color, Vector2 pos)
    {
        var go = new GameObject("Triangle");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateTriangleSprite();
        sr.color = color;
        sr.transform.localScale = Vector3.one * shapeSize * 2f;

        var col = go.AddComponent<PolygonCollider2D>();
        col.SetPath(0, new Vector2[]
        {
            new Vector2(-0.5f, -0.433f),
            new Vector2( 0.5f, -0.433f),
            new Vector2( 0f,    0.433f)
        });
        AddRigidbody(go);

        return go;
    }

    private static void AddRigidbody(GameObject go)
    {
        var rb = go.AddComponent<Rigidbody2D>();
        rb.mass = 1f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void ConnectShapes(GameObject from, GameObject to)
    {
        var spring = to.AddComponent<SpringJoint2D>();
        spring.connectedBody = from.GetComponent<Rigidbody2D>();
        spring.autoConfigureDistance = false;
        spring.distance = spacing;
        spring.frequency = 100f;
        spring.dampingRatio = 1f;
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null && _chain.Contains(hit.gameObject))
            {
                _dragTarget = hit.gameObject;
                var rb = _dragTarget.GetComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                _dragOffset = (Vector2)_dragTarget.transform.position - worldPos;
            }
        }

        if (Input.GetMouseButton(0) && _dragTarget != null)
        {
            Vector2 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            _dragTarget.GetComponent<Rigidbody2D>().MovePosition(worldPos + _dragOffset);
        }

        if (Input.GetMouseButtonUp(0) && _dragTarget != null)
        {
            var rb = _dragTarget.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            _dragTarget = null;
        }
    }

    private static Sprite CreateSquareSprite()
    {
        var tex = new Texture2D(64, 64);
        var pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
    }

    private static Sprite CreateCircleSprite()
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = center - 1f;
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - center + 0.5f;
            float dy = y - center + 0.5f;
            pixels[y * size + x] = (dx * dx + dy * dy <= radius * radius)
                ? Color.white : Color.clear;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), (float)size);
    }

    private static Sprite CreateTriangleSprite()
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];

        Vector2 v0 = new Vector2(0, 0);
        Vector2 v1 = new Vector2(size - 1, 0);
        Vector2 v2 = new Vector2(size / 2f, size - 1);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            var p = new Vector2(x, y);
            pixels[y * size + x] = PointInTriangle(p, v0, v1, v2)
                ? Color.white : Color.clear;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), (float)size);
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        => (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
}