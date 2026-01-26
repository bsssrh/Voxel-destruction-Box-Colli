using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VoxelDestructionPro.Minimal.Editor
{
    [CustomEditor(typeof(MinimalVoxLoader))]
    public class MinimalVoxLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MinimalVoxLoader loader = (MinimalVoxLoader)target;

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(Application.isPlaying && !loader.isActiveAndEnabled))
            {
                if (GUILayout.Button("Load Vox From Path"))
                {
                    bool loaded = loader.LoadAndApply(true);
                    if (loaded)
                    {
                        EditorUtility.SetDirty(loader);
                        if (loader.gameObject.scene.IsValid())
                            EditorSceneManager.MarkSceneDirty(loader.gameObject.scene);
                    }
                }
            }
        }
    }
}
