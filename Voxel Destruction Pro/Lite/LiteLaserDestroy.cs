using UnityEngine;
using VoxelDestructionPro.VoxelObjects;

namespace VoxelDestructionPro.Lite
{
    /// <summary>
    /// Lite laser: shoots a ray from a camera/transform and destroys voxels on hit.
    /// </summary>
    public class LiteLaserDestroy : MonoBehaviour
    {
        [Tooltip("Ray origin; if null, Camera.main is used.")]
        public Transform rayOrigin;

        [Tooltip("Max distance for the ray.")]
        public float maxDistance = 200f;

        private const float DestructionRadius = 2f;

        private void Update()
        {
            if (!Input.GetMouseButton(0))
                return;

            Transform origin = rayOrigin != null
                ? rayOrigin
                : (Camera.main != null ? Camera.main.transform : null);

            if (origin == null)
                return;

            Ray ray = new Ray(origin.position, origin.forward);

            if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return;

            DynamicVoxelObj voxelObj = hit.collider.GetComponentInParent<DynamicVoxelObj>();
            if (voxelObj == null)
                return;

            voxelObj.AddDestruction_Sphere(hit.point, DestructionRadius);
        }
    }
}
