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
    }

    public PoolItem[] poolItems;

    private Dictionary<GameObject, Transform> parentMap = new Dictionary<GameObject, Transform>();
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary;
    private Dictionary<GameObject, GameObject> prefabMap;
    private bool initialized = false;

    // Awake artýk hiçbir þey yapmýyor - AddComponent sýrasýndaki
    // erken/boþ çalýþma sorununu önlemek için kurulum Init()'e taþýndý.
    void Awake() { }

    public void Init()
    {
        if (initialized) return;
        initialized = true;

        poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        prefabMap = new Dictionary<GameObject, GameObject>();

        if (poolItems == null) return;

        foreach (var item in poolItems)
        {
            if (item == null || item.prefab == null) continue; // boþ slotlarý atla

            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < item.initialSize; i++)
            {
                GameObject obj = Instantiate(item.prefab, item.parent);
                obj.SetActive(false);
                queue.Enqueue(obj);
                prefabMap[obj] = item.prefab;
            }
            if (!poolDictionary.ContainsKey(item.prefab))
                poolDictionary.Add(item.prefab, queue);
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!initialized) Init(); // güvenlik aðý

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
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.SetActive(true);
        return objToSpawn;
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