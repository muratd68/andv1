using UnityEngine;
using System.Collections.Generic;

namespace WaterSort.Core
{
    /// <summary>
    /// High-performance object pooling system
    /// Prevents excessive Instantiate/Destroy calls that cause GC spikes
    /// CRITICAL for mobile performance
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        private static ObjectPoolManager _instance;
        public static ObjectPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ObjectPoolManager");
                    _instance = go.AddComponent<ObjectPoolManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Pool dictionary - each prefab has its own pool
        private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> _prefabReferences = new Dictionary<string, GameObject>();
        private Dictionary<GameObject, string> _activeObjects = new Dictionary<GameObject, string>();

        [Header("Pool Settings")]
        [Tooltip("Pre-warm pools on start for better first-time performance")]
        public bool PreWarmPools = true;

        [Tooltip("Maximum objects per pool to prevent memory bloat")]
        public int MaxPoolSize = 100;

        [Header("Statistics (Debug)")]
        public int TotalPooledObjects = 0;
        public int TotalActiveObjects = 0;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Create a pool for a specific prefab
        /// </summary>
        public void CreatePool(string poolName, GameObject prefab, int initialSize = 10)
        {
            if (_pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"Pool '{poolName}' already exists");
                return;
            }

            _pools[poolName] = new Queue<GameObject>();
            _prefabReferences[poolName] = prefab;

            // Pre-warm the pool
            if (PreWarmPools)
            {
                for (int i = 0; i < initialSize; i++)
                {
                    GameObject obj = CreateNewObject(poolName, prefab);
                    obj.SetActive(false);
                    _pools[poolName].Enqueue(obj);
                }

                TotalPooledObjects += initialSize;
            }

            Debug.Log($"Pool '{poolName}' created with {initialSize} pre-warmed objects");
        }

        /// <summary>
        /// Spawn an object from the pool
        /// </summary>
        public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!_pools.ContainsKey(poolName))
            {
                Debug.LogError($"Pool '{poolName}' does not exist. Create it first with CreatePool()");
                return null;
            }

            GameObject obj;

            // Reuse from pool if available
            if (_pools[poolName].Count > 0)
            {
                obj = _pools[poolName].Dequeue();
                TotalPooledObjects--;
            }
            else
            {
                // Create new if pool is empty
                obj = CreateNewObject(poolName, _prefabReferences[poolName]);
            }

            // Setup object
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.SetParent(parent);
            obj.SetActive(true);

            // Track active object
            _activeObjects[obj] = poolName;
            TotalActiveObjects++;

            return obj;
        }

        /// <summary>
        /// Spawn with default rotation
        /// </summary>
        public GameObject Spawn(string poolName, Vector3 position, Transform parent = null)
        {
            return Spawn(poolName, position, Quaternion.identity, parent);
        }

        /// <summary>
        /// Spawn at origin
        /// </summary>
        public GameObject Spawn(string poolName, Transform parent = null)
        {
            return Spawn(poolName, Vector3.zero, Quaternion.identity, parent);
        }

        /// <summary>
        /// Return object to pool (instead of Destroy)
        /// </summary>
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            if (!_activeObjects.ContainsKey(obj))
            {
                Debug.LogWarning("Trying to despawn an object that wasn't spawned from pool");
                Destroy(obj);
                return;
            }

            string poolName = _activeObjects[obj];
            _activeObjects.Remove(obj);
            TotalActiveObjects--;

            // Check pool size limit
            if (_pools[poolName].Count >= MaxPoolSize)
            {
                Destroy(obj);
                return;
            }

            // Reset object state
            obj.SetActive(false);
            obj.transform.SetParent(transform); // Keep hierarchy clean

            // Return to pool
            _pools[poolName].Enqueue(obj);
            TotalPooledObjects++;
        }

        /// <summary>
        /// Despawn after delay
        /// </summary>
        public void Despawn(GameObject obj, float delay)
        {
            if (obj != null)
            {
                StartCoroutine(DespawnAfterDelay(obj, delay));
            }
        }

        private System.Collections.IEnumerator DespawnAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            Despawn(obj);
        }

        /// <summary>
        /// Clear a specific pool
        /// </summary>
        public void ClearPool(string poolName)
        {
            if (!_pools.ContainsKey(poolName))
                return;

            while (_pools[poolName].Count > 0)
            {
                GameObject obj = _pools[poolName].Dequeue();
                Destroy(obj);
                TotalPooledObjects--;
            }

            _pools.Remove(poolName);
            _prefabReferences.Remove(poolName);
        }

        /// <summary>
        /// Clear all pools - use when changing scenes
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    Destroy(obj);
                }
            }

            _pools.Clear();
            _prefabReferences.Clear();
            _activeObjects.Clear();

            TotalPooledObjects = 0;
            TotalActiveObjects = 0;

            Debug.Log("All object pools cleared");
        }

        private GameObject CreateNewObject(string poolName, GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.name = $"{poolName}_{obj.GetInstanceID()}";
            return obj;
        }

        /// <summary>
        /// Get pool statistics for debugging
        /// </summary>
        public void PrintPoolStats()
        {
            Debug.Log("=== Object Pool Statistics ===");
            Debug.Log($"Total Pooled: {TotalPooledObjects}");
            Debug.Log($"Total Active: {TotalActiveObjects}");

            foreach (var kvp in _pools)
            {
                Debug.Log($"Pool '{kvp.Key}': {kvp.Value.Count} available");
            }
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }

        #region Helper Methods

        /// <summary>
        /// Check if a pool exists
        /// </summary>
        public bool PoolExists(string poolName)
        {
            return _pools.ContainsKey(poolName);
        }

        /// <summary>
        /// Get number of available objects in pool
        /// </summary>
        public int GetPoolSize(string poolName)
        {
            return _pools.ContainsKey(poolName) ? _pools[poolName].Count : 0;
        }

        /// <summary>
        /// Pre-allocate more objects to a pool
        /// </summary>
        public void ExpandPool(string poolName, int additionalSize)
        {
            if (!_pools.ContainsKey(poolName))
            {
                Debug.LogError($"Pool '{poolName}' does not exist");
                return;
            }

            GameObject prefab = _prefabReferences[poolName];

            for (int i = 0; i < additionalSize; i++)
            {
                GameObject obj = CreateNewObject(poolName, prefab);
                obj.SetActive(false);
                _pools[poolName].Enqueue(obj);
                TotalPooledObjects++;
            }

            Debug.Log($"Pool '{poolName}' expanded by {additionalSize} objects");
        }

        #endregion
    }
}
