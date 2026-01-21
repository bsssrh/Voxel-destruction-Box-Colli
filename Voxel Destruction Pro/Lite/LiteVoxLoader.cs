using UnityEngine;
using VoxelDestructionPro.VoxelObjects;
using VoxelDestructionPro.VoxDataProviders;

namespace VoxelDestructionPro.Lite
{
    /// <summary>
    /// Minimal loader for a single .vox model.
    /// Uses VoxFileDataProvider under the hood, but exposes a tiny API.
    /// </summary>
    [RequireComponent(typeof(DynamicVoxelObj))]
    public class LiteVoxLoader : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("Drag & drop a .vox file here (Editor only).")]
        public DefaultAsset voxFile;

        [Tooltip("Fallback path inside StreamingAssets (without extension).")]
        public string modelPath;

        [Tooltip("Model index inside the .vox file.")]
        [Min(0)]
        public int modelIndex;

        [Header("Behavior")]
        public bool loadOnStart = true;

        private DynamicVoxelObj voxelObj;
        private VoxFileDataProvider dataProvider;

        private void Awake()
        {
            voxelObj = GetComponent<DynamicVoxelObj>();
            dataProvider = GetComponent<VoxFileDataProvider>();

            if (dataProvider == null)
                dataProvider = gameObject.AddComponent<VoxFileDataProvider>();

            EnsureMeshCollider();
        }

        private void Start()
        {
            if (!loadOnStart)
                return;

            Load();
        }

        public void Load()
        {
            if (voxelObj == null || dataProvider == null)
                return;

            ApplyLiteDefaults();
            ApplySourceSettings();

            dataProvider.Load(false);
        }

        private void ApplyLiteDefaults()
        {
            LiteVoxelManager manager = LiteVoxelManager.Instance;
            if (manager == null)
                return;

            manager.ApplyDefaults(voxelObj);
        }

        private void ApplySourceSettings()
        {
            dataProvider.voxFile = voxFile;
            dataProvider.modelPath = modelPath;
            dataProvider.modelIndex = modelIndex;
            dataProvider.useModelCaching = false;
        }

        private void EnsureMeshCollider()
        {
            if (voxelObj.targetCollider is MeshCollider)
                return;

            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();

            meshCollider.convex = false;
            voxelObj.targetCollider = meshCollider;
        }
    }
}
