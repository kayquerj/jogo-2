using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Level
{
    [ExecuteAlways]
    public class IslandLevelGreybox : MonoBehaviour
    {
        [System.Serializable]
        public class ExpansionZone
        {
            public string id = "Zone";
            public Vector3 centerOffset = new Vector3(0f, 0f, 24f);
            public float platformRadius = 8f;
            public float platformThickness = 1.2f;
            public float gateHeight = 3.2f;
            public float gateThickness = 0.7f;
            public float gateOffsetFromPlatform = 1.6f;
            public float bridgeWidth = 4f;
            public float bridgeThickness = 0.45f;
            public float activationOffsetFromBridge = 2.5f;
            public float resourceRingRadius = 3.4f;
            public int resourceNodes = 3;
            public float resourceNodeScale = 1.2f;
            public ResourceNodeMarker.ResourceType resourceType = ResourceNodeMarker.ResourceType.Wood;
        }

        [Header("Island Footprint")]
        [SerializeField] private float islandRadius = 18f;
        [SerializeField] private float islandThickness = 1.4f;
        [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0.75f, -6.5f);

        [Header("Zone Layout")]
        [SerializeField] private List<ExpansionZone> expansionZones = new List<ExpansionZone>
        {
            new ExpansionZone
            {
                id = "Harbor",
                centerOffset = new Vector3(0f, 0f, 24f),
                resourceNodes = 4,
                resourceRingRadius = 3.8f,
                resourceType = ResourceNodeMarker.ResourceType.Wood
            },
            new ExpansionZone
            {
                id = "Cliffside",
                centerOffset = new Vector3(-20f, 0f, 14f),
                resourceType = ResourceNodeMarker.ResourceType.Stone,
                gateOffsetFromPlatform = 1.8f,
                bridgeWidth = 4.4f,
                activationOffsetFromBridge = 2.2f
            },
            new ExpansionZone
            {
                id = "Lagoon",
                centerOffset = new Vector3(20f, 0f, 14f),
                resourceType = ResourceNodeMarker.ResourceType.Crystal,
                gateOffsetFromPlatform = 1.5f,
                bridgeWidth = 4.2f,
                activationOffsetFromBridge = 2.0f
            }
        };

        [Header("Materials")]
        [SerializeField] private Material islandMaterial;
        [SerializeField] private Material zoneMaterial;
        [SerializeField] private Material bridgeMaterial;
        [SerializeField] private Material gateMaterial;
        [SerializeField] private Material resourceMaterial;

        [Header("Generation")]
        [SerializeField] private bool autoRebuild = true;
        [SerializeField] private string generatedRootName = "Generated Geometry";

        private static readonly Vector3 GizmoHeightOffset = new Vector3(0f, 0.05f, 0f);

        private void OnEnable()
        {
            if (autoRebuild)
            {
                BuildLevel();
            }
        }

        private void Start()
        {
            if (!autoRebuild)
            {
                BuildLevel();
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += () =>
                {
                    if (this == null || !autoRebuild)
                    {
                        return;
                    }

                    BuildLevel();
                };
            }
#endif
        }

        public void BuildLevel()
        {
            var root = GetOrCreateRoot();
            ClearChildren(root);

            BuildCentralIsland(root);
            CreateSpawnMarker(root);

            foreach (var zone in expansionZones)
            {
                BuildExpansionZone(root, zone);
            }
        }

        private Transform GetOrCreateRoot()
        {
            var existing = transform.Find(generatedRootName);
            if (existing == null)
            {
                var go = new GameObject(generatedRootName)
                {
                    isStatic = true
                };
                go.transform.SetParent(transform, false);
                existing = go.transform;
            }

            return existing;
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
                else
#endif
                {
                    Object.Destroy(child.gameObject);
                }
            }
        }

        private void BuildCentralIsland(Transform root)
        {
            var island = CreatePrimitive(PrimitiveType.Cylinder, "CentralIsland", root,
                Vector3.up * (islandThickness * 0.5f));
            island.transform.localScale = new Vector3(islandRadius, islandThickness * 0.5f, islandRadius);
            island.isStatic = true;
            ApplyMaterial(island, islandMaterial, new Color(0.35f, 0.68f, 0.51f));

            var collider = island.GetComponent<Collider>();
            if (collider == null)
            {
                collider = island.AddComponent<MeshCollider>();
            }

            collider.sharedMaterial = null;
        }

        private void CreateSpawnMarker(Transform root)
        {
            var spawnRoot = new GameObject("PlayerSpawn")
            {
                isStatic = false
            };
            spawnRoot.transform.SetParent(root, false);
            spawnRoot.transform.localPosition = spawnPosition;
            spawnRoot.AddComponent<PlayerSpawnPoint>();

            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Visual";
            capsule.transform.SetParent(spawnRoot.transform, false);
            capsule.transform.localScale = new Vector3(0.9f, 1.6f, 0.9f);
            capsule.transform.localPosition = Vector3.zero;
            ApplyMaterial(capsule, resourceMaterial, new Color(0.18f, 0.55f, 0.94f));

            var visualCollider = capsule.GetComponent<Collider>();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(visualCollider);
            }
            else
#endif
            {
                Object.Destroy(visualCollider);
            }

            var trigger = spawnRoot.AddComponent<CapsuleCollider>();
            trigger.isTrigger = true;
            trigger.height = 2.4f;
            trigger.radius = 0.7f;
            trigger.center = new Vector3(0f, 1.2f, 0f);
        }

        private void BuildExpansionZone(Transform root, ExpansionZone zone)
        {
            var zoneRoot = new GameObject(zone.id)
            {
                isStatic = false
            };
            zoneRoot.transform.SetParent(root, false);
            zoneRoot.transform.localPosition = Vector3.zero;

            Vector3 direction = zone.centerOffset;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }
            direction = direction.normalized;

            var platform = CreatePrimitive(PrimitiveType.Cylinder, $"{zone.id}_Platform", zoneRoot.transform,
                zone.centerOffset + Vector3.up * (zone.platformThickness * 0.5f));
            platform.transform.localScale = new Vector3(zone.platformRadius, zone.platformThickness * 0.5f, zone.platformRadius);
            platform.isStatic = true;
            ApplyMaterial(platform, zoneMaterial, new Color(0.62f, 0.69f, 0.49f));

            var platformCollider = platform.GetComponent<Collider>();
            if (platformCollider == null)
            {
                platformCollider = platform.AddComponent<MeshCollider>();
            }

            ConfigureBridgeAndGate(zoneRoot.transform, direction, zone);
            CreateResourceCluster(zoneRoot.transform, zone);
        }

        private void ConfigureBridgeAndGate(Transform zoneRoot, Vector3 direction, ExpansionZone zone)
        {
            Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Vector3 platformCenter = zone.centerOffset;

            float walkwayMidHeight = ((islandThickness + zone.platformThickness) * 0.5f) - (zone.bridgeThickness * 0.5f);
            float walkwayFloorHeight = walkwayMidHeight - (zone.bridgeThickness * 0.5f);

            Vector3 gateBase = platformCenter - forward * (zone.platformRadius + zone.gateOffsetFromPlatform);
            gateBase.y = walkwayFloorHeight;
            Vector3 gateCenter = gateBase + Vector3.up * (zone.gateHeight * 0.5f);

            Vector3 baseEdge = forward * Mathf.Max(1f, islandRadius - 1.2f);
            baseEdge.y = walkwayFloorHeight;

            Vector3 gateBridgeAnchor = gateBase + Vector3.up * (zone.bridgeThickness * 0.5f);
            Vector3 baseBridgeAnchor = baseEdge + Vector3.up * (zone.bridgeThickness * 0.5f);
            float bridgeLength = Mathf.Max(1f, Vector3.Distance(gateBridgeAnchor, baseBridgeAnchor));
            Vector3 bridgeCenter = (gateBridgeAnchor + baseBridgeAnchor) * 0.5f;

            var bridge = CreatePrimitive(PrimitiveType.Cube, $"{zone.id}_Bridge", zoneRoot, bridgeCenter);
            bridge.transform.localScale = new Vector3(zone.bridgeWidth, zone.bridgeThickness, bridgeLength);
            bridge.transform.localRotation = Quaternion.LookRotation(forward, Vector3.up);
            bridge.isStatic = true;
            ApplyMaterial(bridge, bridgeMaterial, new Color(0.53f, 0.53f, 0.57f));

            var bridgeCollider = bridge.GetComponent<BoxCollider>();
            bridgeCollider.isTrigger = false;

            var gate = CreatePrimitive(PrimitiveType.Cube, $"{zone.id}_Gate", zoneRoot, gateCenter);
            gate.transform.localScale = new Vector3(zone.bridgeWidth * 0.8f, zone.gateHeight, zone.gateThickness);
            gate.transform.localRotation = Quaternion.LookRotation(forward, Vector3.up);
            ApplyMaterial(gate, gateMaterial, new Color(0.89f, 0.62f, 0.32f));

            var gateCollider = gate.GetComponent<BoxCollider>();
            gateCollider.isTrigger = true;

            var gateMarker = gate.AddComponent<GateMarker>();
            gateMarker.zoneId = zone.id;
            gateMarker.clearanceHeight = zone.gateHeight;

            Vector3 activationBase = baseEdge - forward * zone.activationOffsetFromBridge;
            activationBase.y = walkwayFloorHeight;

            var activation = new GameObject($"{zone.id}_BridgeActivation")
            {
                isStatic = false
            };
            activation.transform.SetParent(zoneRoot, false);
            activation.transform.localPosition = activationBase;
            activation.transform.localRotation = Quaternion.LookRotation(forward, Vector3.up);

            var activationCollider = activation.AddComponent<BoxCollider>();
            activationCollider.isTrigger = true;
            activationCollider.size = new Vector3(zone.bridgeWidth * 0.7f, 1.4f, 1.6f);
            activationCollider.center = new Vector3(0f, 0.7f, 0f);

            var activationMarker = activation.AddComponent<BridgeActivationZone>();
            activationMarker.zoneId = zone.id;
            activationMarker.bridgeLength = bridgeLength;
            activationMarker.activationSize = activationCollider.size;
        }

        private void CreateResourceCluster(Transform zoneRoot, ExpansionZone zone)
        {
            if (zone.resourceNodes <= 0)
            {
                return;
            }

            var resources = new GameObject("ResourceNodes")
            {
                isStatic = false
            };
            resources.transform.SetParent(zoneRoot, false);
            resources.transform.localPosition = Vector3.zero;

            float angleStep = 360f / zone.resourceNodes;
            for (int i = 0; i < zone.resourceNodes; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * zone.resourceRingRadius;
                Vector3 nodePosition = zone.centerOffset + offset + Vector3.up * (zone.platformThickness + zone.resourceNodeScale * 0.5f);

                var node = CreatePrimitive(PrimitiveType.Capsule, $"{zone.id}_Resource_{i + 1}", resources.transform, nodePosition);
                node.transform.localScale = new Vector3(zone.resourceNodeScale, zone.resourceNodeScale, zone.resourceNodeScale);
                ApplyMaterial(node, resourceMaterial, GetResourceTint(zone.resourceType));

                var collider = node.GetComponent<CapsuleCollider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                    collider.radius = 0.45f;
                    collider.height = 2.2f;
                    collider.center = new Vector3(0f, 1.1f, 0f);
                }

                var marker = node.AddComponent<ResourceNodeMarker>();
                marker.resourceType = zone.resourceType;
                marker.interactionRadius = Mathf.Max(1.5f, zone.resourceRingRadius * 0.35f);
            }
        }

        private static Color GetResourceTint(ResourceNodeMarker.ResourceType type)
        {
            return type switch
            {
                ResourceNodeMarker.ResourceType.Wood => new Color(0.49f, 0.72f, 0.41f),
                ResourceNodeMarker.ResourceType.Stone => new Color(0.58f, 0.6f, 0.63f),
                ResourceNodeMarker.ResourceType.Crystal => new Color(0.37f, 0.62f, 0.94f),
                _ => new Color(0.49f, 0.72f, 0.41f)
            };
        }

        private static GameObject CreatePrimitive(PrimitiveType primitive, string name, Transform parent, Vector3 localPosition)
        {
            var go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.layer = LayerMask.NameToLayer("Default");
            return go;
        }

        private static Shader CachedStandardShader;

        private void ApplyMaterial(GameObject target, Material preferred, Color fallbackColor)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            if (preferred != null)
            {
                renderer.sharedMaterial = preferred;
                return;
            }

            if (CachedStandardShader == null)
            {
                CachedStandardShader = Shader.Find("Standard");
            }

            if (CachedStandardShader == null)
            {
                return;
            }

            var material = renderer.sharedMaterial;
            bool createNew = material == null || material == renderer.material;
            if (createNew)
            {
                material = new Material(CachedStandardShader);
            }
            else if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                material = new Material(material);
#endif
            }

            material.color = fallbackColor;
            renderer.sharedMaterial = material;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (expansionZones == null)
            {
                return;
            }

            Handles.color = new Color(0.4f, 0.7f, 1f, 0.2f);
            Handles.DrawWireDisc(transform.position, Vector3.up, islandRadius);

            foreach (var zone in expansionZones)
            {
                Vector3 center = transform.position + zone.centerOffset + GizmoHeightOffset;
                Handles.DrawWireDisc(center, Vector3.up, zone.platformRadius);
                Handles.DrawLine(transform.position + GizmoHeightOffset,
                    transform.position + zone.centerOffset + GizmoHeightOffset);
            }
        }
#endif
    }
}
