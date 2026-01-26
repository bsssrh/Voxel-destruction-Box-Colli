using System.IO;
using UnityEngine;
using VoxReader.Interfaces;

namespace VoxelDestructionPro.Minimal
{
    public class MinimalVoxLoader : MonoBehaviour
    {
        [Header("Vox Source")]
        [SerializeField] private string streamingAssetPath;
        [SerializeField] private int modelIndex;
        [SerializeField] private bool loadOnStart = true;

        [Header("Target")]
        [SerializeField] private MinimalVoxelObject targetObject;

        private void Start()
        {
            if (!loadOnStart)
                return;

            LoadAndApply(true);
        }

        public bool LoadAndApply(bool log = false)
        {
            if (targetObject == null)
                targetObject = GetComponent<MinimalVoxelObject>();

            if (targetObject == null)
            {
                if (log)
                    Debug.LogWarning("MinimalVoxLoader: Missing MinimalVoxelObject target.", this);
                return false;
            }

            MinimalVoxelData data = LoadVoxelData(log);
            if (data == null)
                return false;

            targetObject.AssignVoxelData(data);

            if (log)
                Debug.Log($"MinimalVoxLoader: Loaded '{streamingAssetPath}' (model {modelIndex}) into '{targetObject.name}'.", this);

            return true;
        }

        public MinimalVoxelData LoadVoxelData(bool log = false)
        {
            if (string.IsNullOrWhiteSpace(streamingAssetPath))
            {
                if (log)
                    Debug.LogWarning("MinimalVoxLoader: Streaming asset path is empty.", this);
                return null;
            }

            string fullPath = GetFullPath(streamingAssetPath);
            if (!File.Exists(fullPath))
            {
                if (log)
                    Debug.LogWarning($"MinimalVoxLoader: Vox file not found at '{fullPath}'.", this);
                return null;
            }

            IVoxFile file = VoxReader.VoxReader.Read(fullPath, false);
            if (file?.Models == null || file.Models.Length == 0)
            {
                if (log)
                    Debug.LogWarning($"MinimalVoxLoader: No models found in '{fullPath}'.", this);
                return null;
            }

            int clampedIndex = Mathf.Clamp(modelIndex, 0, file.Models.Length - 1);
            return MinimalVoxelData.FromModel(file.Models[clampedIndex]);
        }

        private static string GetFullPath(string path)
        {
            string resolvedPath = path;
            if (!resolvedPath.EndsWith(".vox"))
                resolvedPath += ".vox";

            return Path.Combine(Application.streamingAssetsPath, resolvedPath);
        }
    }
}
