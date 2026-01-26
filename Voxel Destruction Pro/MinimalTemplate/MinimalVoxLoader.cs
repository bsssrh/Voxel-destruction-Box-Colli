using System.IO;
using UnityEngine;
using VoxReader.Interfaces;

namespace VoxelDestructionPro.Minimal
{
    [ExecuteAlways]
    public class MinimalVoxLoader : MonoBehaviour
    {
        [Header("Vox Source")]
        [SerializeField] private string streamingAssetPath;
        [SerializeField] private int modelIndex;

        [Header("Target")]
        [SerializeField] private MinimalVoxelObject targetObject;

        [ContextMenu("Load Vox (Editor)")]
        public void LoadVoxInEditor()
        {
            if (!Application.isEditor)
            {
                Debug.LogWarning("MinimalVoxLoader: Editor-only load was called outside the editor.", this);
                return;
            }

            Debug.Log($"MinimalVoxLoader: Loading .vox from StreamingAssets path '{streamingAssetPath}'.", this);
            MinimalVoxelData data = LoadVoxelData();
            if (data == null)
            {
                Debug.LogWarning("MinimalVoxLoader: Load failed. Check the path, model index, and logs above.", this);
                return;
            }

            if (targetObject == null)
                targetObject = GetComponent<MinimalVoxelObject>();

            if (targetObject == null)
            {
                Debug.LogError("MinimalVoxLoader: No MinimalVoxelObject found to assign data.", this);
                return;
            }

            targetObject.AssignVoxelData(data);
            Debug.Log($"MinimalVoxLoader: Loaded voxels {data.Volume} with palette size {data.palette?.Length ?? 0}.", this);
        }

        public MinimalVoxelData LoadVoxelData()
        {
            IVoxFile file = null;

            if (!string.IsNullOrWhiteSpace(streamingAssetPath))
            {
                string path = streamingAssetPath;
                if (!path.EndsWith(".vox"))
                    path += ".vox";

                string fullPath = Path.Combine(Application.streamingAssetsPath, path);

                if (!File.Exists(fullPath))
                {
                    Debug.LogError($"MinimalVoxLoader: File does not exist at '{fullPath}'.", this);
                    return null;
                }

                try
                {
                    file = VoxReader.VoxReader.Read(fullPath, false);
                }
                catch (IOException ioException)
                {
                    Debug.LogError($"MinimalVoxLoader: IO error reading '{fullPath}': {ioException.Message}", this);
                    return null;
                }
                catch (System.Exception exception)
                {
                    Debug.LogError($"MinimalVoxLoader: Failed to parse vox file '{fullPath}': {exception.Message}", this);
                    return null;
                }
            }
            else
            {
                Debug.LogWarning("MinimalVoxLoader: StreamingAssets path is empty.", this);
                return null;
            }

            if (file?.Models == null || file.Models.Length == 0)
            {
                Debug.LogError("MinimalVoxLoader: Vox file has no models.", this);
                return null;
            }

            int clampedIndex = Mathf.Clamp(modelIndex, 0, file.Models.Length - 1);
            return MinimalVoxelData.FromModel(file.Models[clampedIndex]);
        }
    }
}
