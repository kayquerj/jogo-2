using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Level
{
    [ExecuteAlways]
    [DefaultExecutionOrder(300)]
    public class RuntimeNavMeshBaker : MonoBehaviour
    {
        [SerializeField] private Vector3 boundsSize = new Vector3(140f, 40f, 140f);
        [SerializeField] private LayerMask includedLayers = ~0;
        [SerializeField] private bool autoBake = true;

        private NavMeshData navMeshData;
        private NavMeshDataInstance navMeshInstance;

        private void OnEnable()
        {
            if (autoBake)
            {
                Bake();
            }
        }

        private void Start()
        {
            if (!autoBake)
            {
                Bake();
            }
        }

        private void OnDisable()
        {
            if (navMeshInstance.valid)
            {
                navMeshInstance.Remove();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && autoBake)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        Bake();
                    }
                };
            }
        }
#endif

        public void Bake()
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            NavMeshBuilder.CollectSources(transform, includedLayers,
                NavMeshCollectGeometry.RenderMeshes, 0, markups, sources);

            if (navMeshData == null)
            {
                navMeshData = new NavMeshData();
            }

            var defaultSettings = NavMesh.GetSettingsByIndex(0);
            var bounds = new Bounds(transform.position, boundsSize);

            if (navMeshInstance.valid)
            {
                navMeshInstance.Remove();
            }

            NavMeshBuilder.UpdateNavMeshData(navMeshData, defaultSettings, sources, bounds);
            navMeshInstance = NavMesh.AddNavMeshData(navMeshData, transform.position, transform.rotation);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.1f, 0.8f, 0.4f, 0.25f);
            Gizmos.DrawWireCube(transform.position, boundsSize);
        }
    }
}
