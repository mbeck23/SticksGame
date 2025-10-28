using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


public class ObjectSpawner : MonoBehaviour
{
    public enum ObjectType { Enemy }

    [Header("Level + Prefabs")]
    public Tilemap tilemap;                 // Tilemap used to detect valid spawn tiles
    public GameObject[] objectPrefabs;      // Prefab array indexed by ObjectType

    [Header("Spawn Limits")]
    public int maxObjects = 5;              // Max number of spawned objects alive at once
    public float spawnInterval = 0.5f;      // Delay between spawns while filling to cap
    private readonly List<Vector3> validSpawnPositions = new List<Vector3>(); // Precomputed positions
    private readonly List<GameObject> spawnObjects = new List<GameObject>();  // Live spawned instances
    private bool isSpawning = false;                                           // Prevents concurrent loops

    [Header("Physics")]
    [SerializeField] private LayerMask groundLayer; // Ground mask for overlap checks
    [SerializeField] private float skin = 0.02f;    // Upward nudge to resolve ground overlaps
    private float enemyHalfHeight;                  // Estimated half-height for spawn offset (informational)

    [Header("Player proximity spawn/despawn")]
    [SerializeField] private Transform player;            // Player reference (auto-found if null)
    [SerializeField] private float respawnDistance = 25f; // Despawn when farther than this
    [SerializeField] private float spawnNearRadius = 8f;  // Prefer spawns within this X radius
    [SerializeField] private float spawnMinDistance = 2f; // But not closer than this to player


    // ------------------------------------------------------------------------

    private void Awake()
    {
        // Try to estimate enemy half-height from the prefab collider (fallback 0.5)
        var enemyCol = objectPrefabs[(int)ObjectType.Enemy]?.GetComponent<Collider2D>();
        enemyHalfHeight = enemyCol ? enemyCol.bounds.extents.y : 0.5f;
    }

    private void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // Build initial spawn points and start filling
        GatherValidPositions();
        StartCoroutine(SpawnObjectsIfNeeded());

        // Additional hook
        GameController.OnReset += LevelChange;
    }

    private void OnEnable()
    {
        // Subscribe to level-related events to refresh spawns
        GameController.OnReset += HandleLevelEvent;
        GameController.OnLevelChanged += HandleLevelEvent;
    }

    private void OnDisable()
    {
        // Unsubscribe and clean up when disabled
        GameController.OnReset -= HandleLevelEvent;
        GameController.OnLevelChanged -= HandleLevelEvent;

        StopAllCoroutines();
        DestroyAllSpawnedObjects();
    }

    private void Update()
    {
        // Regularly despawn far-away objects so they can respawn near player
        DespawnFarEnemies();

        // If below cap and not already running a spawn loop, start one
        if (!isSpawning && ActiveObjectCount() < maxObjects)
        {
            StartCoroutine(SpawnObjectsIfNeeded());
        }
    }

    private void HandleLevelEvent()
    {
        StopAllCoroutines();
        isSpawning = false;

        DestroyAllSpawnedObjects();

        // Refresh tilemap reference 
        var ground = GameObject.Find("Ground");
        if (ground != null) tilemap = ground.GetComponent<Tilemap>();

        GatherValidPositions();
        StartCoroutine(SpawnObjectsIfNeeded());
    }

    private void LevelChange()
    {
        var ground = GameObject.Find("Ground");
        if (ground != null) tilemap = ground.GetComponent<Tilemap>();

        GatherValidPositions();
        DestroyAllSpawnedObjects();
    }

    private int ActiveObjectCount()
    {
        spawnObjects.RemoveAll(item => item == null);
        return spawnObjects.Count;
    }

    private IEnumerator SpawnObjectsIfNeeded()
    {
        isSpawning = true;

        while (ActiveObjectCount() < maxObjects)
        {
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private void SpawnObject()
    {
        if (validSpawnPositions.Count == 0) return;

        // Choose a candidate index, preferring horizontally near the player if available
        int chosenIndex = -1;

        if (player != null)
        {
            List<int> near = new List<int>(validSpawnPositions.Count);

            // Collect indices within the preferred horizontal window 
            for (int i = 0; i < validSpawnPositions.Count; i++)
            {
                var p = validSpawnPositions[i];
                float dx = Mathf.Abs(player.position.x - p.x);
                if (dx >= spawnMinDistance && dx <= spawnNearRadius)
                    near.Add(i);
            }

            // Shuffle the "near" list 
            for (int i = 0; i < near.Count; i++)
            {
                int j = Random.Range(i, near.Count);
                (near[i], near[j]) = (near[j], near[i]);
            }

            if (near.Count > 0) chosenIndex = near[0];
        }

        // Fallback to any position if no "near" option was found
        if (chosenIndex < 0) chosenIndex = Random.Range(0, validSpawnPositions.Count);

        Vector3 spawnPos = validSpawnPositions[chosenIndex];

        // Instantiate exactly at the precomputed “top-of-tile” point
        GameObject go = Instantiate(objectPrefabs[(int)ObjectType.Enemy], spawnPos, Quaternion.identity, transform);
        spawnObjects.Add(go);

        var col = go.GetComponent<Collider2D>();
        if (col)
        {
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = groundLayer,
                useTriggers = false
            };

            Collider2D[] tmp = new Collider2D[2];
            int tries = 0;

            while (col.OverlapCollider(filter, tmp) > 0 && tries < 12)
            {
                go.transform.position += Vector3.up * skin;
                tries++;
            }
        }
    }

    private void DespawnFarEnemies()
    {
        if (player == null) return;

        for (int i = spawnObjects.Count - 1; i >= 0; i--)
        {
            var obj = spawnObjects[i];
            if (obj == null) { spawnObjects.RemoveAt(i); continue; }

            if (Vector2.Distance(player.position, obj.transform.position) > respawnDistance)
            {
                Destroy(obj);
                spawnObjects.RemoveAt(i);
            }
        }
    }

    private void DestroyAllSpawnedObjects()
    {
        foreach (GameObject obj in spawnObjects.ToArray())
        {
            if (obj != null) Destroy(obj);
        }
        spawnObjects.Clear();
    }

    private void GatherValidPositions()
    {
        validSpawnPositions.Clear();

        if (!tilemap)
        {
            Debug.LogWarning("ObjectSpawner: tilemap not set.");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                var above = new Vector3Int(x, y + 1, 0);

                // Ground tile, no tile above it
                if (tilemap.HasTile(cell) && !tilemap.HasTile(above))
                {
                    // World center of the ground cell
                    Vector3 groundCenter = tilemap.GetCellCenterWorld(cell);

                    // Top surface of that tile is half a cell higher in Y
                    float topY = groundCenter.y + tilemap.cellSize.y * 0.5f;

                    // Enemy just above surface
                    Vector3 spawn = new Vector3(groundCenter.x, topY + 0.05f, 0f);

                    validSpawnPositions.Add(spawn);
                }
            }
        }
    }
}
