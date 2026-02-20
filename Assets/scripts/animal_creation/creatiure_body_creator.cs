using UnityEngine;
using System.Collections.Generic;

public class CreatureBodyCreator : MonoBehaviour
{
    // ── Shape Primitives ─────────────────────────────────────────────────────

    public enum ShapeType { Circle, Square, Triangle }

    [System.Serializable]
    public class BodyPart
    {
        public string slotName;
        public ShapeType shape;
        public Vector2 localPosition;
        public Vector2 size;
        public Color color;
        public float rotation;
    }

    // ── Creature Templates ───────────────────────────────────────────────────

    public enum CreatureType { Worm, Bird, Fish, Spider }

    // Each template defines what SLOTS exist and what shapes are ALLOWED per slot
    [System.Serializable]
    public class BodySlot
    {
        public string slotName;
        public ShapeType[] allowedShapes;    // randomly pick one of these
        public Vector2 localPosition;
        public Vector2 sizeMin;
        public Vector2 sizeMax;
        public float rotationMin;
        public float rotationMax;
        public bool optional;                // 50% chance to skip if true
    }

    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Generation")]
    public bool randomizeOnStart = true;
    public int seed = 0;
    public CreatureType forceType;           // only used if randomizeOnStart = false

    [Header("Color")]
    public Color[] palette = new Color[]
    {
        new Color(0.3f, 0.8f, 0.3f),
        new Color(0.8f, 0.4f, 0.2f),
        new Color(0.2f, 0.5f, 0.9f),
        new Color(0.9f, 0.8f, 0.2f),
        new Color(0.7f, 0.3f, 0.8f),
    };

    // ── Runtime ──────────────────────────────────────────────────────────────

    private List<GameObject> spawnedParts = new List<GameObject>();
    private static Dictionary<ShapeType, Sprite> spriteCache = new Dictionary<ShapeType, Sprite>();

    void Start()
    {
        GenerateCreature();
    }

    [ContextMenu("Regenerate")]
    public void GenerateCreature()
    {
        foreach (var p in spawnedParts) Destroy(p);
        spawnedParts.Clear();

        if (randomizeOnStart) seed = Random.Range(0, 99999);
        Random.InitState(seed);

        CreatureType type = randomizeOnStart
            ? (CreatureType)Random.Range(0, System.Enum.GetValues(typeof(CreatureType)).Length)
            : forceType;

        Color baseColor = palette[Random.Range(0, palette.Length)];
        Color accentColor = palette[Random.Range(0, palette.Length)];

        BodySlot[] slots = GetTemplate(type);

        foreach (var slot in slots)
        {
            if (slot.optional && Random.value < 0.5f) continue;

            ShapeType shape = slot.allowedShapes[Random.Range(0, slot.allowedShapes.Length)];
            Vector2 size = new Vector2(
                Random.Range(slot.sizeMin.x, slot.sizeMax.x),
                Random.Range(slot.sizeMin.y, slot.sizeMax.y)
            );
            float rot = Random.Range(slot.rotationMin, slot.rotationMax);

            // Accent color for detail slots (eyes, wings, fins)
            bool isAccent = slot.slotName.Contains("Eye") || slot.slotName.Contains("Wing")
                         || slot.slotName.Contains("Fin") || slot.slotName.Contains("Spot");
            Color color = isAccent ? accentColor : baseColor;

            SpawnPart(slot.slotName, shape, slot.localPosition, size, rot, color);
        }
    }

    // ── Templates ────────────────────────────────────────────────────────────

    BodySlot[] GetTemplate(CreatureType type)
    {
        switch (type)
        {
            case CreatureType.Worm: return WormTemplate();
            case CreatureType.Bird: return BirdTemplate();
            case CreatureType.Fish: return FishTemplate();
            case CreatureType.Spider: return SpiderTemplate();
            default: return WormTemplate();
        }
    }

    // Worm: chained body segments + tiny head
    BodySlot[] WormTemplate() => new BodySlot[]
    {
        new BodySlot { slotName = "Segment1",  allowedShapes = new[]{ ShapeType.Circle, ShapeType.Square }, localPosition = new Vector2(-0.6f, 0),   sizeMin = new Vector2(0.35f,0.35f), sizeMax = new Vector2(0.5f,0.5f) },
        new BodySlot { slotName = "Segment2",  allowedShapes = new[]{ ShapeType.Circle, ShapeType.Square }, localPosition = new Vector2(-0.15f, 0),  sizeMin = new Vector2(0.35f,0.35f), sizeMax = new Vector2(0.5f,0.5f) },
        new BodySlot { slotName = "Segment3",  allowedShapes = new[]{ ShapeType.Circle, ShapeType.Square }, localPosition = new Vector2(0.3f, 0),    sizeMin = new Vector2(0.3f,0.3f),   sizeMax = new Vector2(0.45f,0.45f) },
        new BodySlot { slotName = "Head",      allowedShapes = new[]{ ShapeType.Circle, ShapeType.Square }, localPosition = new Vector2(0.7f, 0.05f),sizeMin = new Vector2(0.3f,0.3f),   sizeMax = new Vector2(0.4f,0.4f) },
        new BodySlot { slotName = "Eye",       allowedShapes = new[]{ ShapeType.Circle },                   localPosition = new Vector2(0.82f,0.12f),sizeMin = new Vector2(0.07f,0.07f), sizeMax = new Vector2(0.1f,0.1f) },
    };

    // Bird: body + head + two triangle wings + triangle beak
    BodySlot[] BirdTemplate() => new BodySlot[]
    {
        new BodySlot { slotName = "Body",      allowedShapes = new[]{ ShapeType.Circle, ShapeType.Square }, localPosition = new Vector2(0, 0),       sizeMin = new Vector2(0.5f,0.35f),  sizeMax = new Vector2(0.7f,0.5f) },
        new BodySlot { slotName = "Head",      allowedShapes = new[]{ ShapeType.Circle, ShapeType.Square }, localPosition = new Vector2(0.45f,0.25f),sizeMin = new Vector2(0.25f,0.25f), sizeMax = new Vector2(0.35f,0.35f) },
        new BodySlot { slotName = "WingTop",   allowedShapes = new[]{ ShapeType.Triangle },                 localPosition = new Vector2(-0.1f,0.3f), sizeMin = new Vector2(0.4f,0.3f),   sizeMax = new Vector2(0.6f,0.45f),  rotationMin = -20, rotationMax = 20 },
        new BodySlot { slotName = "WingBot",   allowedShapes = new[]{ ShapeType.Triangle },                 localPosition = new Vector2(-0.1f,-0.3f),sizeMin = new Vector2(0.4f,0.3f),   sizeMax = new Vector2(0.6f,0.45f),  rotationMin = 160, rotationMax = 200 },
        new BodySlot { slotName = "Beak",      allowedShapes = new[]{ ShapeType.Triangle },                 localPosition = new Vector2(0.68f,0.22f),sizeMin = new Vector2(0.15f,0.1f),  sizeMax = new Vector2(0.22f,0.15f), rotationMin = -10, rotationMax = 10 },
        new BodySlot { slotName = "Eye",       allowedShapes = new[]{ ShapeType.Circle },                   localPosition = new Vector2(0.54f,0.31f),sizeMin = new Vector2(0.06f,0.06f), sizeMax = new Vector2(0.09f,0.09f) },
        new BodySlot { slotName = "Tail",      allowedShapes = new[]{ ShapeType.Triangle, ShapeType.Square},localPosition = new Vector2(-0.55f,0),   sizeMin = new Vector2(0.2f,0.15f),  sizeMax = new Vector2(0.3f,0.25f),  rotationMin = 170, rotationMax = 190, optional = true },
    };

    // Fish: oval body + triangle tail fin + triangle top fin + small eye
    BodySlot[] FishTemplate() => new BodySlot[]
    {
        new BodySlot { slotName = "Body",      allowedShapes = new[]{ ShapeType.Circle, ShapeType.Square }, localPosition = new Vector2(0, 0),        sizeMin = new Vector2(0.6f,0.35f),  sizeMax = new Vector2(0.85f,0.55f) },
        new BodySlot { slotName = "TailFin",   allowedShapes = new[]{ ShapeType.Triangle },                 localPosition = new Vector2(-0.6f, 0),    sizeMin = new Vector2(0.25f,0.3f),  sizeMax = new Vector2(0.4f,0.45f),  rotationMin = 170, rotationMax = 190 },
        new BodySlot { slotName = "TopFin",    allowedShapes = new[]{ ShapeType.Triangle },                 localPosition = new Vector2(0.05f, 0.38f),sizeMin = new Vector2(0.2f,0.2f),   sizeMax = new Vector2(0.3f,0.3f),   rotationMin = -10, rotationMax = 10 },
        new BodySlot { slotName = "Eye",       allowedShapes = new[]{ ShapeType.Circle },                   localPosition = new Vector2(0.32f,0.1f),  sizeMin = new Vector2(0.07f,0.07f), sizeMax = new Vector2(0.11f,0.11f) },
        new BodySlot { slotName = "Spot",      allowedShapes = new[]{ ShapeType.Circle, ShapeType.Square }, localPosition = new Vector2(-0.05f,0.05f),sizeMin = new Vector2(0.1f,0.1f),   sizeMax = new Vector2(0.2f,0.2f),   optional = true },
    };

    // Spider: round body + smaller head + 4 pairs of triangle/square legs
    BodySlot[] SpiderTemplate() => new BodySlot[]
    {
        new BodySlot { slotName = "Abdomen",   allowedShapes = new[]{ ShapeType.Circle, ShapeType.Square }, localPosition = new Vector2(-0.3f,0),     sizeMin = new Vector2(0.45f,0.4f),  sizeMax = new Vector2(0.6f,0.55f) },
        new BodySlot { slotName = "Head",      allowedShapes = new[]{ ShapeType.Circle, ShapeType.Square }, localPosition = new Vector2(0.2f,0),      sizeMin = new Vector2(0.28f,0.28f), sizeMax = new Vector2(0.38f,0.38f) },
        new BodySlot { slotName = "Eye1",      allowedShapes = new[]{ ShapeType.Circle },                   localPosition = new Vector2(0.28f,0.1f),  sizeMin = new Vector2(0.05f,0.05f), sizeMax = new Vector2(0.08f,0.08f) },
        new BodySlot { slotName = "Eye2",      allowedShapes = new[]{ ShapeType.Circle },                   localPosition = new Vector2(0.34f,0.1f),  sizeMin = new Vector2(0.05f,0.05f), sizeMax = new Vector2(0.08f,0.08f) },
        new BodySlot { slotName = "LegFL",     allowedShapes = new[]{ ShapeType.Square, ShapeType.Triangle},localPosition = new Vector2(0.15f, 0.28f),sizeMin = new Vector2(0.08f,0.35f), sizeMax = new Vector2(0.1f,0.45f),  rotationMin = -30, rotationMax = -10 },
        new BodySlot { slotName = "LegFR",     allowedShapes = new[]{ ShapeType.Square, ShapeType.Triangle},localPosition = new Vector2(0.15f,-0.28f),sizeMin = new Vector2(0.08f,0.35f), sizeMax = new Vector2(0.1f,0.45f),  rotationMin = 10,  rotationMax = 30 },
        new BodySlot { slotName = "LegML",     allowedShapes = new[]{ ShapeType.Square, ShapeType.Triangle},localPosition = new Vector2(-0.05f,0.3f), sizeMin = new Vector2(0.08f,0.35f), sizeMax = new Vector2(0.1f,0.45f),  rotationMin = -20, rotationMax = 5 },
        new BodySlot { slotName = "LegMR",     allowedShapes = new[]{ ShapeType.Square, ShapeType.Triangle},localPosition = new Vector2(-0.05f,-0.3f),sizeMin = new Vector2(0.08f,0.35f), sizeMax = new Vector2(0.1f,0.45f),  rotationMin = -5,  rotationMax = 20 },
        new BodySlot { slotName = "LegBL",     allowedShapes = new[]{ ShapeType.Square, ShapeType.Triangle},localPosition = new Vector2(-0.25f,0.28f),sizeMin = new Vector2(0.08f,0.35f), sizeMax = new Vector2(0.1f,0.45f),  rotationMin = -10, rotationMax = 20 },
        new BodySlot { slotName = "LegBR",     allowedShapes = new[]{ ShapeType.Square, ShapeType.Triangle},localPosition = new Vector2(-0.25f,-0.28f),sizeMin= new Vector2(0.08f,0.35f), sizeMax = new Vector2(0.1f,0.45f),  rotationMin = -20, rotationMax = 10 },
    };

    // ── Spawning ─────────────────────────────────────────────────────────────

    void SpawnPart(string partName, ShapeType shape, Vector2 pos, Vector2 size, float rot, Color color)
    {
        var go = new GameObject(partName);
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        go.transform.localScale = new Vector3(size.x, size.y, 1);
        go.transform.localRotation = Quaternion.Euler(0, 0, rot);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSprite(shape);
        sr.color = color;

        // Sort order: eyes on top, legs behind body
        if (partName.Contains("Eye"))   sr.sortingOrder = 3;
        else if (partName.Contains("Leg")) sr.sortingOrder = 0;
        else if (partName.Contains("Wing") || partName.Contains("Fin") || partName.Contains("Tail")) sr.sortingOrder = 1;
        else sr.sortingOrder = 2;

        spawnedParts.Add(go);
    }

    // ── Sprite Generation ─────────────────────────────────────────────────────

    Sprite GetSprite(ShapeType shape)
    {
        if (spriteCache.TryGetValue(shape, out var cached)) return cached;

        Sprite sprite = shape switch
        {
            ShapeType.Circle   => MakeCircleSprite(64),
            ShapeType.Square   => MakeSquareSprite(64),
            ShapeType.Triangle => MakeTriangleSprite(64),
            _                  => MakeCircleSprite(64),
        };

        spriteCache[shape] = sprite;
        return sprite;
    }

    Sprite MakeCircleSprite(int res)
    {
        var tex = new Texture2D(res, res);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 c = Vector2.one * (res / 2f);
        float r = res / 2f;
        for (int x = 0; x < res; x++)
            for (int y = 0; y < res; y++)
                tex.SetPixel(x, y, Vector2.Distance(new Vector2(x, y), c) <= r ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,res,res), Vector2.one * 0.5f, res);
    }

    Sprite MakeSquareSprite(int res)
    {
        var tex = new Texture2D(res, res);
        tex.filterMode = FilterMode.Bilinear;
        for (int x = 0; x < res; x++)
            for (int y = 0; y < res; y++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,res,res), Vector2.one * 0.5f, res);
    }

    Sprite MakeTriangleSprite(int res)
    {
        var tex = new Texture2D(res, res);
        tex.filterMode = FilterMode.Bilinear;
        // Fill transparent first
        for (int x = 0; x < res; x++)
            for (int y = 0; y < res; y++)
                tex.SetPixel(x, y, Color.clear);

        // Triangle pointing right: bottom-left, top-left, mid-right
        Vector2 p0 = new Vector2(0, 0);
        Vector2 p1 = new Vector2(0, res);
        Vector2 p2 = new Vector2(res, res / 2f);

        for (int x = 0; x < res; x++)
            for (int y = 0; y < res; y++)
                if (PointInTriangle(new Vector2(x, y), p0, p1, p2))
                    tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,res,res), Vector2.one * 0.5f, res);
    }

    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p,a,b), d2 = Sign(p,b,c), d3 = Sign(p,c,a);
        bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
        bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
        return !(hasNeg && hasPos);
    }

    float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        => (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
}