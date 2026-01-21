using UnityEngine;
using VoxelDestructionPro.VoxelObjects;

namespace VoxelDestructionPro.Lite
{
    /// <summary>
    /// Simple laser destruction: raycast -> destroy a small sphere.
    /// Minimal settings, no paint or filters.
    /// </summary>
    public class LiteLaserDestroy : MonoBehaviour
    {
        [Header("Ray Source")]
        public Camera rayCamera;
        public Transform rayOrigin;

        [Header("Settings")]
        [Min(0.1f)]
        public float maxDistance = 200f;
        [Min(0.01f)]
        public float destructionRadius = 1f;
        public KeyCode fireKey = KeyCode.Mouse0;

        private void Update()
        {
            if (!Input.GetKeyDown(fireKey))
                return;

            Ray ray = BuildRay();
            if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance))
                return;

            DynamicVoxelObj voxelObj = hit.transform.GetComponentInParent<DynamicVoxelObj>();
            if (voxelObj == null)
                return;

            voxelObj.AddDestruction_Sphere(hit.point, destructionRadius);
        }

        private Ray BuildRay()
        {
            if (rayCamera != null)
                return rayCamera.ScreenPointToRay(Input.mousePosition);

            if (rayOrigin != null)
                return new Ray(rayOrigin.position, rayOrigin.forward);

            return new Ray(transform.position, transform.forward);
        }
    }
}
