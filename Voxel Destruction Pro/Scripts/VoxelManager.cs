using System;
using System.Collections.Generic;
using System.Linq;
using Better.StreamingAssets;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelDestructionPro.Data;
using VoxelDestructionPro.Data.Serializable;
using VoxelDestructionPro.Settings;
using VoxelDestructionPro.Tools;

namespace VoxelDestructionPro
{
    /// <summary>
    /// Manages stuff that is independent of a single voxel object
    ///
    /// - Quick setup
    /// - Voxeldata caching
    /// - Fragment pooling
    /// </summary>
    public class VoxelManager : MonoBehaviour
    {
        private static VoxelManager _Instance;

        public static VoxelManager Instance
        {
            get
            {
                if (_Instance == null)
                    Debug.LogError("There is no Voxel Manager in the scene!");

                return _Instance;
            }
        }
        
        public enum ColliderType
        {
            None, Standard, Convex
        }
        
        [Header("Quick setup")]
        
        public Material standardMaterial;
        public ColliderType standardCollider;
        public MeshSettingsObj standardMeshSettings;
        public IsoSettings standardIsolationSettings;
        public DynSettings standardDynamicSettings;
        public Transform fragmentParent;

        [Header("Fragment pooling")] 
        
        public PooledFragments[] fragmentPools;

        //Main object pool
        private bool poolChanged;
        private Dictionary<GameObject, Queue<GameObject>> objectPool;
        
        //Vox obj caching
        private Dictionary<Tuple<string, int>, CachedVoxelData> voxelCache;

        private bool betterStreamingAssetsLoaded;
        
        private void Awake()
        {
            if (_Instance == null)
            {
                _Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_Instance != this)
            {
                Destroy(gameObject);
            }
            
            betterStreamingAssetsLoaded = false;

            InitPooling();
        }

        #region VoxCaching
        
        public void LoadBSA()
        {
            //I think it is better if we dont load it multiple times
            if (betterStreamingAssetsLoaded)
                return;

            betterStreamingAssetsLoaded = true;
            BetterStreamingAssets.Initialize();
        }
        
        /// <summary>
        /// Loads Voxeldata and caches it, this allows
        /// the reuse of Voxeldata
        /// </summary>
        /// <param name="modelpath"></param>
        /// <param name="modelIndex"></param>
        /// <returns></returns>
        public VoxelData LoadAndCacheVoxFile(string modelpath, int modelIndex)
        {
            voxelCache ??= new Dictionary<Tuple<string, int>, CachedVoxelData>();
            
            Tuple<string, int> key = new Tuple<string, int>(modelpath, modelIndex);
            if (voxelCache.ContainsKey(key))
            {
                //Aleady cached, we dont need to load
                return new VoxelData(voxelCache[key].GetCopy());
            }

            VoxelParser parser = new VoxelParser(modelpath, modelIndex);
            VoxelData file = parser.ParseToVoxelData();
            voxelCache.Add(key, file.ToCachedVoxelData().GetCopy());

            return file;
        }
        
        #endregion

        #region FragmentPool

        private void InitPooling()
        {
            objectPool = new Dictionary<GameObject, Queue<GameObject>>();

            foreach (var pool in fragmentPools)
            {
                if (pool.instanceCount <= 0)
                    continue;

                var instances = new Queue<GameObject>();

                for (int i = 0; i < pool.instanceCount; i++)
                {
                    GameObject n = Instantiate(pool.prefab, transform, true);
                    n.SetActive(false);
                    instances.Enqueue(n);
                }
                
                objectPool.Add(pool.prefab, instances);
            }

            poolChanged = false;
        }
        
        /// <inheritdoc cref="InstantiatePooled(UnityEngine.GameObject,UnityEngine.Vector3,UnityEngine.Quaternion)"/>
        public GameObject InstantiatePooled(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            GameObject newObj = InstantiatePooled(prefab);
            newObj.transform.SetPositionAndRotation(pos, rot);

            return newObj;
        }

        /// <summary>
        /// Checks if the requested object is pooled, else it Instantiates it
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public GameObject InstantiatePooled(GameObject prefab)
        {
            if (objectPool.TryGetValue(prefab, out var pooled))
            {
                if (pooled.TryDequeue(out GameObject res))
                {
                    if (res == null)
                        return Instantiate(prefab);
                    
                    poolChanged = true;
                    res.SetActive(true);
                    res.transform.parent = null;
                    return res;
                }
                else 
                    return Instantiate(prefab);
            }
            else
                return Instantiate(prefab);
        }

        /// <summary>
        /// Updates the object pool by adding new elements when they got removed
        /// </summary>
        private void UpdatePooling()
        {
            if (!poolChanged)
                return;
            
            poolChanged = false;
            
            foreach (var kvp in objectPool)
            {
                int targetCount = fragmentPools.First(t => t.prefab == kvp.Key).instanceCount;

                if (kvp.Value.Count < targetCount)
                {
                    GameObject n = Instantiate(kvp.Key, transform, true);
                    n.SetActive(false);
                    kvp.Value.Enqueue(n);
                    poolChanged = true;
                }
            }
        }
        
        #endregion

        #region Events
        
        private void Update()
        {
            UpdatePooling();
        }

        #endregion
    }
}