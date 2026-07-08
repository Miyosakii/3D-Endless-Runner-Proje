using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolItem
    {
        public GameObject prefab;
        public int initialSize = 20;
        public Transform parent;
        public float heightOffset = 0f; // Her prefab için özel yükseklik ayarý
    }

    public PoolItem[] poolItems;

    private Dictionary<GameObject, Transform> parentMap = new Dictionary<GameObject, Transform>();
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary;
    private Dictionary<GameObject, GameObject> prefabMap;
    private Dictionary<GameObject, float> prefabHeightMap; // Prefab'lerin yüksekliklerini tutacak
    private bool initialized = false;

    void Awake() { }

    public void Init()
    {
        if (initialized) return;
        initialized = true;

        poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        prefabMap = new Dictionary<GameObject, GameObject>();
        prefabHeightMap = new Dictionary<GameObject, float>();

        if (poolItems == null) return;

        foreach (var item in poolItems)
        {
            if (item == null || item.prefab == null) continue;

            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < item.initialSize; i++)
            {
                GameObject obj = Instantiate(item.prefab, item.parent);

                // Sýfýrlama
                obj.transform.localPosition = Vector3.zero;

                obj.SetActive(false);
                queue.Enqueue(obj);
                prefabMap[obj] = item.prefab;
            }

            if (!poolDictionary.ContainsKey(item.prefab))
            {
                poolDictionary.Add(item.prefab, queue);
                prefabHeightMap.Add(item.prefab, item.heightOffset); // Offset'i kaydet
            }
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!initialized) Init();

        if (prefab == null || !poolDictionary.ContainsKey(prefab))
        {
            Debug.LogError($"[ObjectPool] {(prefab != null ? prefab.name : "NULL")} havuzda yok!");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[prefab];

        if (queue.Count == 0)
        {
            Transform parent = parentMap.ContainsKey(prefab) ? parentMap[prefab] : null;
            GameObject newObj = Instantiate(prefab, parent);

            newObj.SetActive(false);
            queue.Enqueue(newObj);
            prefabMap[newObj] = prefab;
        }

        GameObject objToSpawn = queue.Dequeue();

        // Her objenin kendi heightOffset deðerini y eksenine ekle
        if (prefabHeightMap != null && prefabHeightMap.ContainsKey(prefab))
        {
            position.y += prefabHeightMap[prefab];
        }

        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.SetActive(true);
        return objToSpawn;
    }

    // Havuzdaki tanýmlý prefablerden rastgele birini seçip döndürür
    public GameObject GetRandom(Vector3 position, Quaternion rotation)
    {
        if (poolItems == null || poolItems.Length == 0) return null;

        // Rastgele bir PoolItem seç
        int randomIndex = Random.Range(0, poolItems.Length);
        GameObject selectedPrefab = poolItems[randomIndex].prefab;

        // Mevcut Get metodunu kullanarak objeyi çaðýr
        return Get(selectedPrefab, position, rotation);
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;
        if (!prefabMap.ContainsKey(obj))
        {
            Destroy(obj);
            return;
        }

        GameObject prefab = prefabMap[obj];
        if (poolDictionary.ContainsKey(prefab))
        {
            obj.SetActive(false);
            poolDictionary[prefab].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}