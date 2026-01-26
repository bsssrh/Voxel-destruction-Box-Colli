using System.IO;
using UnityEngine;
using VoxReader.Interfaces;

namespace VoxelDestructionPro.Minimal
{
    public class MinimalVoxLoader : MonoBehaviour
    {
        [Header("Vox Source")]
        [SerializeField] private TextAsset voxAsset;
        [SerializeField] private string streamingAssetPath;
        [SerializeField] private int modelIndex;
        [SerializeField] private bool loadOnStart = true;

        [Header("Target")]
        [SerializeField] private MinimalVoxelObject targetObject;

        private void Start()
        {
            if (!loadOnStart)
                return;

            MinimalVoxelData data = LoadVoxelData();
            if (data != null)
            {
                if (targetObject == null)
                    targetObject = GetComponent<MinimalVoxelObject>();

                if (targetObject != null)
                    targetObject.AssignVoxelData(data);
            }
        }

        public MinimalVoxelData LoadVoxelData()
        {
            IVoxFile file = null;

            if (voxAsset != null && voxAsset.bytes != null && voxAsset.bytes.Length > 0)
            {
                file = VoxReader.VoxReader.Read(voxAsset.bytes);
            }
            else if (!string.IsNullOrWhiteSpace(streamingAssetPath))
            {
                string path = streamingAssetPath;
                if (!path.EndsWith(".vox"))
                    path += ".vox";

                string fullPath = Path.Combine(Application.streamingAssetsPath, path);

                if (File.Exists(fullPath))
                    file = VoxReader.VoxReader.Read(fullPath, false);
            }

            if (file?.Models == null || file.Models.Length == 0)
                return null;

            int clampedIndex = Mathf.Clamp(modelIndex, 0, file.Models.Length - 1);
            return MinimalVoxelData.FromModel(file.Models[clampedIndex]);
        }
    }
}
