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

    public GameObject eye;
    public GameObject[] features;
    public float featureSpawnChange = 0.5f;
    public int numCreatures = 4;
    public GameObject background;

    // Each creature is a list of its shapes
    private List<List<GameObject>> _creatures = new List<List<GameObject>>();
    private List<GameObject> _parents = new List<GameObject>();

    private GameObject _dragTarget;
    private Vector2 _dragOffset;
    private Camera _cam;

    private enum ShapeType { Circle, Square, Triangle }

    private void Start()
    {
        _cam = Camera.main;
        spacing = 2f * shapeSize;
        for (int i = 0; i < numCreatures; i++)
        {
            SpawnChain();
        }
    }

    private void Update()
    {
        HandleMouseDrag();
    }

    public void Respawn()
    {
        DestroyAll();
        for (int i = 0; i < numCreatures; i++)
        {
            SpawnChain();
        }
    }

    private void SpawnChain()
    {
        int count = Random.Range(1, 6);
        List<GameObject> chain = new List<GameObject>();

        GameObject parent = new GameObject("Creature_" + _parents.Count);
        _parents.Add(parent);

        Vector2 origin = spawnOrigin;
        if (background != null)
        {
            Bounds b = background.GetComponent<Renderer>().bounds;
            float chainWidth = (count - 1) * spacing;
            origin = new Vector2(
                Random.Range(b.min.x, b.max.x - chainWidth),
                Random.Range(b.min.y, b.max.y)
            );
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = origin + Vector2.right * (i * spacing);
            ShapeType type = (ShapeType)Random.Range(0, 3);
            Color color = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f);

            GameObject shape = CreateShape(type, color, pos, parent);
            chain.Add(shape);

            if (i == 0)
                AttachEye(shape);

            if (i == count - 1)
                AttachEye(shape);

            if (i > 0)
            {
                ConnectShapes(chain[i - 1], shape);
                AttachRandomAccessory(shape);
            }
        }

        if (anchorFirstShape && chain.Count > 0)
            chain[0].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        var target = parent.AddComponent<PhotoTarget>();
        target.basePoints = Random.Range(50, 200);
        _creatures.Add(chain);
    }

    private void DestroyAll()
    {
        foreach (var p in _parents)
            if (p != null) Destroy(p);
        _parents.Clear();
        _creatures.Clear();
    }

    private GameObject CreateShape(ShapeType type, Color color, Vector2 position, GameObject parent)
    {
        switch (type)
        {
            case ShapeType.Circle:   return CreateCircle(color, position, parent);
            case ShapeType.Square:   return CreateSquare(color, position, parent);
            default:                 return CreateTriangle(color, position, parent);
        }
    }

    private GameObject CreateCircle(Color color, Vector2 pos, GameObject parent)
    {
        var go = new GameObject("Circle");
        go.transform.SetParent(parent.transform, false);
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

    private GameObject CreateSquare(Color color, Vector2 pos, GameObject parent)
    {
        var go = new GameObject("Square");
        go.transform.SetParent(parent.transform, false);
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

    private GameObject CreateTriangle(Color color, Vector2 pos, GameObject parent)
    {
        var go = new GameObject("Triangle");
        go.transform.SetParent(parent.transform, false);
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
        rb.freezeRotation = true;
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

    // Find which creature a shape belongs to
    private bool IsInAnyCreature(GameObject go)
    {
        foreach (var chain in _creatures)
            if (chain.Contains(go)) return true;
        return false;
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null && IsInAnyCreature(hit.gameObject))
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

    private void AttachRandomAccessory(GameObject shape)
    {
        if (features.Length == 0 || Random.Range(0f, 1f) < 0.5f)
            return;

        int feature = Random.Range(0, features.Length);
        GameObject prefab = features[feature];
        Bounds bounds = shape.GetComponent<Renderer>().bounds;

        Vector2 randomOffset = new Vector2(
            Random.Range(-bounds.extents.x * 0.15f, bounds.extents.x * 0.15f),
            Random.Range(-bounds.extents.y * 0.15f, bounds.extents.y * 0.15f)
        );

        GameObject newFeature = Instantiate(prefab, shape.transform);
        newFeature.transform.localPosition = new Vector3(randomOffset.x, randomOffset.y, -5f);
    }

    private void AttachEye(GameObject shape)
    {
        int numEyes = Random.Range(0, 4);
        for (int i = 0; i < numEyes; i++)
        {
            Bounds bounds = shape.GetComponent<Renderer>().bounds;

            Vector2 randomOffset = new Vector2(
                Random.Range(-bounds.extents.x * 0.15f, bounds.extents.x * 0.15f),
                Random.Range(-bounds.extents.y * 0.15f, bounds.extents.y * 0.15f)
            );

            GameObject newFeature = Instantiate(eye, shape.transform);
            newFeature.transform.localPosition = new Vector3(randomOffset.x, randomOffset.y, -5f);
        }
    }
}