using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Track Settings")]
    [SerializeField] private GameObject trackPrefab;
    [SerializeField] private float trackLength = 10f;
    [SerializeField] private int initialTrackCount = 5;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private int blocksAheadBuffer = 3;   // <-- yeni: kaç blok önde dursun
    [SerializeField] private int blocksBehindBuffer = 2;   // <-- yeni: kaç blok arkada tutulsun

    private float spawnAheadDistance;
    private float despawnBehindDistance;

    // Yeni ayarlar: blok yüksekliði ve lane aralýðý / lane sayýsý
    [SerializeField] private float trackHeightOffset = 1f;
    [SerializeField] private float laneSpacing = 2.5f;

    [Header("Obstacle & Collectible")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private GameObject[] collectiblePrefabs;
    [SerializeField] private float spawnProbability = 0.3f;

    private ObjectPool trackPool;
    private ObjectPool obstaclePool;
    private ObjectPool collectiblePool;
    private int laneCount = 3;


    private float lastSpawnZ;
    private List<GameObject> activeTracks = new List<GameObject>();

    private void Start()
    {
        spawnAheadDistance = trackLength * blocksAheadBuffer;
        despawnBehindDistance = trackLength * blocksBehindBuffer;

        // Track havuzu
        trackPool = gameObject.AddComponent<ObjectPool>();
        trackPool.poolItems = new ObjectPool.PoolItem[]
        {
            new ObjectPool.PoolItem { prefab = trackPrefab, initialSize = 10, parent = this.transform }
        };
        trackPool.Init(); // <-- artýk poolItems atandýktan SONRA kuruluyor

        // Engel havuzu (eðer prefab varsa)
        if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
        {
            var obstacleItems = new List<ObjectPool.PoolItem>();
            foreach (var prefab in obstaclePrefabs)
            {
                if (prefab != null)
                    obstacleItems.Add(new ObjectPool.PoolItem { prefab = prefab, initialSize = 5, parent = this.transform });
            }
            obstaclePool = gameObject.AddComponent<ObjectPool>();
            obstaclePool.poolItems = obstacleItems.ToArray();
            obstaclePool.Init();
        }

        // Collectible havuzu (eðer prefab varsa)
        if (collectiblePrefabs != null && collectiblePrefabs.Length > 0)
        {
            var collectibleItems = new List<ObjectPool.PoolItem>();
            foreach (var prefab in collectiblePrefabs)
            {
                if (prefab != null)
                    collectibleItems.Add(new ObjectPool.PoolItem { prefab = prefab, initialSize = 5, parent = this.transform });
            }
            collectiblePool = gameObject.AddComponent<ObjectPool>();
            collectiblePool.poolItems = collectibleItems.ToArray();
            collectiblePool.Init();
        }

        // Baþlangýç bloklarý
        for (int i = 0; i < initialTrackCount; i++)
            SpawnTrackBlock(i * trackLength);
        lastSpawnZ = initialTrackCount * trackLength;
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        float playerZ = playerTransform.position.z;
        float spawnThreshold = playerZ + spawnAheadDistance;
        float despawnThreshold = playerZ - despawnBehindDistance;

        while (lastSpawnZ < spawnThreshold)
        {
            SpawnTrackBlock(lastSpawnZ);
            lastSpawnZ += trackLength;
        }

        for (int i = activeTracks.Count - 1; i >= 0; i--)
        {
            GameObject track = activeTracks[i];
            if (track.transform.position.z < despawnThreshold)
            {
                foreach (Transform child in track.transform)
                {
                    if (child.CompareTag("Obstacle") && obstaclePool != null)
                        obstaclePool.Return(child.gameObject);
                    else if (child.CompareTag("Collectible") && collectiblePool != null)
                        collectiblePool.Return(child.gameObject);
                    else
                        Destroy(child.gameObject);
                }
                trackPool.Return(track);
                activeTracks.RemoveAt(i);
            }
        }
    }

    private void SpawnTrackBlock(float zPos)
    {
        // Yükseklik offset'i uygulanýyor
        Vector3 pos = new Vector3(0, trackHeightOffset, zPos);
        GameObject block = trackPool.Get(trackPrefab, pos, Quaternion.identity);
        activeTracks.Add(block);
        PopulateTrackBlock(block);
    }

    private void PopulateTrackBlock(GameObject block)
    {
        int objectCount = Random.Range(1, 4);
        // center hesaplama: laneCount esnekliði için
        float center = (laneCount - 1) / 2f;

        for (int i = 0; i < objectCount; i++)
        {
            int lane = Random.Range(0, laneCount);
            float laneX = (lane - center) * laneSpacing;
            float zOffset = Random.Range(1f, trackLength - 1f);

            // Objelerin yerleþtirileceði yükseklik: önceki 0.5f + track yüksekliði
            float itemY = 0.5f + trackHeightOffset;
            Vector3 spawnPos = new Vector3(laneX, itemY, block.transform.position.z + zOffset);

            // Engel mi collectible mi?
            if (Random.value < spawnProbability)
            {
                if (obstaclePrefabs != null && obstaclePrefabs.Length > 0 && obstaclePool != null)
                {
                    // Null olmayan bir prefab seç
                    List<GameObject> validPrefabs = new List<GameObject>();
                    foreach (var prefab in obstaclePrefabs)
                    {
                        if (prefab != null) validPrefabs.Add(prefab);
                    }
                    if (validPrefabs.Count > 0)
                    {
                        GameObject prefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
                        GameObject obj = obstaclePool.Get(prefab, spawnPos, Quaternion.identity);
                        if (obj != null)
                        {
                            obj.transform.SetParent(block.transform);
                            obj.SetActive(true);
                        }
                    }
                }
            }
            else
            {
                if (collectiblePrefabs != null && collectiblePrefabs.Length > 0 && collectiblePool != null)
                {
                    List<GameObject> validPrefabs = new List<GameObject>();
                    foreach (var prefab in collectiblePrefabs)
                    {
                        if (prefab != null) validPrefabs.Add(prefab);
                    }
                    if (validPrefabs.Count > 0)
                    {
                        GameObject prefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
                        GameObject obj = collectiblePool.Get(prefab, spawnPos, Quaternion.identity);
                        if (obj != null)
                        {
                            obj.transform.SetParent(block.transform);
                            obj.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}