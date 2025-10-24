using UnityEngine;

namespace Game.Level
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.6f, 1f, 0.65f);
        [SerializeField] private float gizmoHeight = 2.2f;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, 0.35f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * gizmoHeight);
        }
    }

    public class GateMarker : MonoBehaviour
    {
        public string zoneId = "Zone";
        public float clearanceHeight = 3f;
        [SerializeField] private Color gizmoColor = new Color(1f, 0.58f, 0.2f, 0.45f);

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            var bounds = GetComponent<Collider>()?.bounds ?? new Bounds(transform.position, Vector3.one);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.DrawLine(bounds.center, bounds.center + Vector3.up * clearanceHeight);
        }
    }

    public class BridgeActivationZone : MonoBehaviour
    {
        public string zoneId = "Zone";
        public float bridgeLength = 6f;
        public Vector3 activationSize = new Vector3(2f, 1.2f, 2f);
        [SerializeField] private Color gizmoColor = new Color(0.3f, 0.85f, 1f, 0.35f);

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Matrix4x4 previous = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, activationSize);
            Gizmos.matrix = previous;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * (bridgeLength * 0.5f));
        }
    }

    public class ResourceNodeMarker : MonoBehaviour
    {
        public enum ResourceType
        {
            Wood,
            Stone,
            Crystal
        }

        public ResourceType resourceType = ResourceType.Wood;
        public float interactionRadius = 1.5f;
        [SerializeField] private Color gizmoColor = new Color(0.4f, 0.8f, 0.5f, 0.4f);

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
