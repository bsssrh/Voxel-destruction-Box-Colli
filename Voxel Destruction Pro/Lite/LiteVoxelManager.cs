using UnityEngine;
using VoxelDestructionPro.Settings;
using VoxelDestructionPro.VoxelObjects;

namespace VoxelDestructionPro.Lite
{
    /// <summary>
    /// Super-light manager with only the required defaults for Lite scripts.
    /// Attach once in the scene and reference the settings you want to use.
    /// </summary>
    public class LiteVoxelManager : MonoBehaviour
    {
        public static LiteVoxelManager Instance { get; private set; }

        [Header("Defaults (minimal)")]
        public Material standardMaterial;
        public MeshSettingsObj meshSettings;
        public IsoSettings isolationSettings;
        public DynSettings dynamicSettings;
        public Transform fragmentParent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void ApplyDefaults(DynamicVoxelObj voxelObj)
        {
            if (voxelObj == null)
                return;

            if (voxelObj.targetFilter != null && standardMaterial != null)
                voxelObj.targetFilter.sharedMaterial = standardMaterial;

            if (meshSettings != null)
                voxelObj.meshSettings = meshSettings;

            if (isolationSettings != null)
                voxelObj.isoSettings = isolationSettings;

            if (dynamicSettings != null)
                voxelObj.dynamicSettings = dynamicSettings;

            if (fragmentParent != null)
                voxelObj.fragmentParent = fragmentParent;
        }
    }
}
