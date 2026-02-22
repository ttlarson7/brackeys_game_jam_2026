using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ShapeChainSpawner : MonoBehaviour
{
    private bool _spawningFlyingCreature = false;
    
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

    [Header("Rendering")]
    public string creatureSortingLayer = "Default";
    public int bodyOrder = 0;
    public int featureOrder = 10;

    [Header("Spawn Bands")] 
    public float skyBandMin = 0.60f;
    public float skyBandMax = 0.95f;
    public float groundBand = 0.08f;

    [Header("Ground Creature Physics")]
    public bool freezeGroundCreatures = true;
    public bool disableGroundGravity = true;
    
    [Header("World Bounds")]
    public bool buildWorldBounds = true;
    public float worldBoundsPadding = 0.5f;
    public float worldBoundsThickness = 1.0f;

    [Header("Testing")] 
    public bool allowDragging = false;
    
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
        if (buildWorldBounds && background != null)
            CreateWorldBoundsFromBackground();
        for (int i = 0; i < numCreatures; i++)
        {
            SpawnChain();
        }
    }
    
    private bool PrefabLooksLikeWing(GameObject prefab)
    {
        if (prefab == null) return false;
        return prefab.name.ToLower().Contains("wing");
    }

    private void Update()
    {
        if (allowDragging)
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

        var sg = parent.AddComponent<SortingGroup>();
        sg.sortingLayerName = "Default";
        sg.sortingOrder = 0;
        
        bool creatureHasWings = false;
        
        bool hasWingPrefab = false;
        for (int f = 0; f < features.Length; f++)
        {
            if (PrefabLooksLikeWing(features[f]))
            {
                hasWingPrefab = true;
                break;
            }
        }
        
        creatureHasWings = hasWingPrefab && (Random.value < 0.5f);

        _spawningFlyingCreature = creatureHasWings;
        
        Vector2 origin = spawnOrigin;
        if (background != null)
        {
            Bounds b = background.GetComponent<Renderer>().bounds;
            float chainWidth = (count - 1) * spacing;
            float height = b.max.y - b.min.y;
            
            float y;
            if (creatureHasWings)
            {
                float yMin = b.min.y + height * skyBandMin;
                float yMax = b.min.y + height * skyBandMax;
                y = Random.Range(yMin, yMax);
            }
            else
            {
                y = b.min.y + height * groundBand;
            }
            
            origin = new Vector2(
                Random.Range(b.min.x, b.max.x - chainWidth),
                y
            );
            _spawningFlyingCreature = false;
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
        SetParentToChainCenter(parent, chain);
        var target = parent.AddComponent<PhotoTarget>();
        target.basePoints = Random.Range(50, 200);
        _creatures.Add(chain);

        if (!creatureHasWings)
        {
            MakeCreatureGroundTarget(chain);
        }

        if (creatureHasWings)
        {
            for (int tries = 0; tries < 6; tries++)
            {
                int idx = Random.Range(0, chain.Count);
                if (TryAttachWing(chain[idx])) break;
            }
        }
    }

    private void MakeCreatureGroundTarget(List<GameObject> chain)
    {
        foreach (var part in chain)
        {
            var rb = part.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            if (disableGroundGravity)
                rb.gravityScale = 0f;

            if (freezeGroundCreatures)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    private bool TryAttachWing(GameObject shape)
    {
        if (features == null || features.Length == 0) return false;
        
        List<GameObject> wings = new List<GameObject>();
        foreach (var f in features)
            if (PrefabLooksLikeWing(f)) wings.Add(f);

        if (wings.Count == 0) return false;

        GameObject prefab = wings[Random.Range(0, wings.Count)];
        Bounds bounds = shape.GetComponent<Renderer>().bounds;

        Vector2 randomOffset = new Vector2(
            Random.Range(-bounds.extents.x * 0.15f, bounds.extents.x * 0.15f),
            Random.Range(-bounds.extents.y * 0.15f, bounds.extents.y * 0.15f)
        );

        GameObject newFeature = Instantiate(prefab, shape.transform);
        newFeature.transform.localPosition = new Vector3(randomOffset.x, randomOffset.y, 0f);

        return true;
    }

    private void CreateWorldBoundsFromBackground()
    {
        Bounds b = background.GetComponent<Renderer>().bounds;

        // Expand bounds slightly so bodies don't clip outside
        float minX = b.min.x - worldBoundsPadding;
        float maxX = b.max.x + worldBoundsPadding;
        float minY = b.min.y - worldBoundsPadding;
        float maxY = b.max.y + worldBoundsPadding;

        GameObject boundsRoot = new GameObject("WorldBounds");
        // Put it at origin so colliders use world positions cleanly
        boundsRoot.transform.position = Vector3.zero;

        // Left wall
        CreateWall(boundsRoot.transform, "Wall_Left",
            new Vector2(minX - worldBoundsThickness * 0.5f, (minY + maxY) * 0.5f),
            new Vector2(worldBoundsThickness, (maxY - minY) + worldBoundsThickness * 2f));

        // Right wall
        CreateWall(boundsRoot.transform, "Wall_Right",
            new Vector2(maxX + worldBoundsThickness * 0.5f, (minY + maxY) * 0.5f),
            new Vector2(worldBoundsThickness, (maxY - minY) + worldBoundsThickness * 2f));

        // Bottom
        CreateWall(boundsRoot.transform, "Wall_Bottom",
            new Vector2((minX + maxX) * 0.5f, minY - worldBoundsThickness * 0.5f),
            new Vector2((maxX - minX) + worldBoundsThickness * 2f, worldBoundsThickness));

        // Top
        CreateWall(boundsRoot.transform, "Wall_Top",
            new Vector2((minX + maxX) * 0.5f, maxY + worldBoundsThickness * 0.5f),
            new Vector2((maxX - minX) + worldBoundsThickness * 2f, worldBoundsThickness));
    }

    private void CreateWall(Transform parent, string name, Vector2 center, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent, false);
        wall.transform.position = new Vector3(center.x, center.y, 0f);

        var col = wall.AddComponent<BoxCollider2D>();
        col.size = size;
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
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 0;
        sr.sortingLayerName = creatureSortingLayer;
        sr.sortingOrder = bodyOrder;
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
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 0;
        sr.sortingLayerName = creatureSortingLayer;
        sr.sortingOrder = bodyOrder;
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
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 0;
        sr.sortingLayerName = creatureSortingLayer;
        sr.sortingOrder = bodyOrder;
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

        // Build a filtered list based on whether this creature is flying
        List<GameObject> pool = new List<GameObject>();

        foreach (var f in features)
        {
            bool isWing = PrefabLooksLikeWing(f);

            // If flying creature: allow wings + non-wings
            // If ground creature: allow ONLY non-wings
            if (_spawningFlyingCreature || !isWing)
                pool.Add(f);
        }

        if (pool.Count == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Count)];

        Bounds bounds = shape.GetComponent<Renderer>().bounds;

        Vector2 randomOffset = new Vector2(
            Random.Range(-bounds.extents.x * 0.15f, bounds.extents.x * 0.15f),
            Random.Range(-bounds.extents.y * 0.15f, bounds.extents.y * 0.15f)
        );

        GameObject newFeature = Instantiate(prefab, shape.transform);
        newFeature.transform.localPosition = new Vector3(randomOffset.x, randomOffset.y, 0f);

        ForceFeatureSorting(newFeature, 10);


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
            newFeature.transform.localPosition = new Vector3(randomOffset.x, randomOffset.y, 0f);

            ForceFeatureSorting(newFeature, 10);



            newFeature.transform.localPosition = new Vector3(randomOffset.x, randomOffset.y, 0f);
        }
    }
    private void SetFeatureRenderLayer(GameObject featureRoot, int orderOffset = 10)
    {
        var renderers = featureRoot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            sr.sortingOrder += orderOffset;
        }
        
        var transforms = featureRoot.GetComponentsInChildren<Transform>(true);
        foreach (var tr in transforms)
        {
            var lp = tr.localPosition;
            tr.localPosition = new Vector3(lp.x, lp.y, 0f);
        }
    }
    
    private void ForceLocalZ(GameObject root, float z)
    {
        foreach (var tr in root.GetComponentsInChildren<Transform>(true))
        {
            var lp = tr.localPosition;
            tr.localPosition = new Vector3(lp.x, lp.y, z);
        }
    }

    private void ForceFeatureSorting(GameObject featureRoot, int orderOffset = 10)
    {
        foreach (var sr in featureRoot.GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder += orderOffset; // keep pupil > white if prefab has it
        }
    }
    private void SetParentToChainCenter(GameObject parent, List<GameObject> chain)
    {
        if (chain == null || chain.Count == 0) return;

        Vector3 sum = Vector3.zero;
        for (int i = 0; i < chain.Count; i++)
            sum += chain[i].transform.position;

        parent.transform.position = sum / chain.Count;
    }
}