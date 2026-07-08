using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Track Settings")]
    [SerializeField] private float trackLength = 10f;
    [SerializeField] private int initialTrackCount = 5;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private int blocksAheadBuffer = 3;
    [SerializeField] private int blocksBehindBuffer = 2;

    private float spawnAheadDistance;
    private float despawnBehindDistance;

    [SerializeField] private float trackHeightOffset = 1f;
    [SerializeField] private float laneSpacing = 2.5f;
    [SerializeField] private float spawnProbability = 0.3f;

    [Header("Pool References")]
    // Artýk doðrudan havuz sistemlerini referans alýyoruz
    [SerializeField] private ObjectPool trackPool;
    [SerializeField] private ObjectPool obstaclePool;
    [SerializeField] private ObjectPool collectiblePool;

    private int laneCount = 3;
    private float lastSpawnZ;
    private List<GameObject> activeTracks = new List<GameObject>();

    private void Start()
    {
        spawnAheadDistance = trackLength * blocksAheadBuffer;
        despawnBehindDistance = trackLength * blocksBehindBuffer;

        // Havuzlarýn Init metotlarýný çaðýrarak hazýr olduklarýndan emin oluyoruz
        if (trackPool != null) trackPool.Init();
        if (obstaclePool != null) obstaclePool.Init();
        if (collectiblePool != null) collectiblePool.Init();

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
        Vector3 pos = new Vector3(0, trackHeightOffset, zPos);

        // Track pool'dan ilk sýradaki prefab'i alýyoruz
        GameObject trackPrefab = trackPool.poolItems[0].prefab;
        GameObject block = trackPool.Get(trackPrefab, pos, Quaternion.identity);

        activeTracks.Add(block);
        PopulateTrackBlock(block);
    }

    private void PopulateTrackBlock(GameObject block)
    {
        int objectCount = Random.Range(1, 4);
        float center = (laneCount - 1) / 2f;

        for (int i = 0; i < objectCount; i++)
        {
            int lane = Random.Range(0, laneCount);
            float laneX = (lane - center) * laneSpacing;
            float zOffset = Random.Range(1f, trackLength - 1f);

            float baseY = block.transform.position.y;
            Vector3 spawnPos = new Vector3(laneX, baseY, block.transform.position.z + zOffset);

            if (Random.value < spawnProbability)
            {
                if (obstaclePool != null)
                {
                    // Yeni eklediðimiz GetRandom metodunu kullanýyoruz
                    GameObject obj = obstaclePool.GetRandom(spawnPos, Quaternion.identity);
                    if (obj != null)
                    {
                        obj.transform.SetParent(block.transform);
                        obj.SetActive(true);
                    }
                }
            }
            else
            {
                if (collectiblePool != null)
                {
                    // Yeni eklediðimiz GetRandom metodunu kullanýyoruz
                    GameObject obj = collectiblePool.GetRandom(spawnPos, Quaternion.identity);
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