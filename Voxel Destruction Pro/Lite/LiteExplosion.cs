using UnityEngine;
using VoxelDestructionPro.VoxelObjects;

namespace VoxelDestructionPro.Lite
{
    /// <summary>
    /// Minimal explosive: wait -> overlap sphere -> destroy nearby voxel objects.
    /// No paint, no material filters, no fancy options.
    /// </summary>
    public class LiteExplosion : MonoBehaviour
    {
        [Min(0f)]
        public float delay = 0.5f;
        [Min(0.1f)]
        public float radius = 6f;

        private float triggerTime;

        private void Start()
        {
            triggerTime = Time.time + delay;
        }

        private void Update()
        {
            if (Time.time < triggerTime)
                return;

            TriggerExplosion();
            Destroy(gameObject);
        }

        private void TriggerExplosion()
        {
            Vector3 origin = transform.position;
            Collider[] colliders = Physics.OverlapSphere(origin, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < colliders.Length; i++)
            {
                DynamicVoxelObj voxelObj = colliders[i].GetComponentInParent<DynamicVoxelObj>();
                if (voxelObj == null)
                    continue;

                voxelObj.AddDestruction_Sphere(origin, radius);
            }
        }
    }
}
