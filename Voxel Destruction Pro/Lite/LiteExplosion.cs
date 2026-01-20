using System.Collections.Generic;
using UnityEngine;
using VoxelDestructionPro.VoxelObjects;

namespace VoxelDestructionPro.Lite
{
    /// <summary>
    /// Lite explosion: single-radius sphere destruction around this object.
    /// </summary>
    public class LiteExplosion : MonoBehaviour
    {
        [Tooltip("Explosion radius used for overlap and destruction.")]
        public float explosionRadius = 6f;

        private readonly HashSet<DynamicVoxelObj> touchedObjects = new HashSet<DynamicVoxelObj>();

        private void Start()
        {
            Explode();
        }

        public void Explode()
        {
            Vector3 position = transform.position;
            Collider[] colliders = Physics.OverlapSphere(
                position,
                explosionRadius,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            );

            touchedObjects.Clear();

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null)
                    continue;

                DynamicVoxelObj vox = collider.GetComponentInParent<DynamicVoxelObj>();
                if (vox == null || touchedObjects.Contains(vox))
                    continue;

                touchedObjects.Add(vox);
                vox.AddDestruction_Sphere(position, explosionRadius);
            }
        }
    }
}
