using System;
using UnityEngine;

namespace Game.Saving
{
    public class ResourceNodeState : MonoBehaviour
    {
        [SerializeField]
        private string nodeId;

        [SerializeField]
        private NodeRunState state = NodeRunState.Inactive;

        [SerializeField, Min(0f)]
        private float remainingSeconds;

        public string NodeId => nodeId;

        public NodeRunState State => state;

        public float RemainingSeconds => remainingSeconds;

        public event Action<ResourceNodeState> NodeStateChanged;

        private void OnEnable()
        {
            GameSaveManager.Instance?.RegisterResourceNode(this);
        }

        private void OnDisable()
        {
            GameSaveManager.Instance?.UnregisterResourceNode(this);
        }

        public void SetState(NodeRunState newState)
        {
            SetState(newState, false);
        }

        public void SetState(NodeRunState newState, bool silent)
        {
            if (state == newState)
            {
                if (!silent)
                {
                    NodeStateChanged?.Invoke(this);
                }

                return;
            }

            state = newState;
            if (!silent)
            {
                NodeStateChanged?.Invoke(this);
            }
        }

        public void SetRemainingSeconds(float seconds)
        {
            SetRemainingSeconds(seconds, false);
        }

        public void SetRemainingSeconds(float seconds, bool silent)
        {
            seconds = Mathf.Max(0f, seconds);
            if (Mathf.Approximately(remainingSeconds, seconds))
            {
                if (!silent)
                {
                    NodeStateChanged?.Invoke(this);
                }

                return;
            }

            remainingSeconds = seconds;
            if (!silent)
            {
                NodeStateChanged?.Invoke(this);
            }
        }

        public void ApplySnapshot(NodeStateData data, bool silent)
        {
            bool changed = false;
            if (state != data.state)
            {
                state = data.state;
                changed = true;
            }

            float clampedTime = Mathf.Max(0f, data.remainingSeconds);
            if (!Mathf.Approximately(remainingSeconds, clampedTime))
            {
                remainingSeconds = clampedTime;
                changed = true;
            }

            if (changed && !silent)
            {
                NodeStateChanged?.Invoke(this);
            }
        }

        public NodeStateData ToSnapshot()
        {
            return new NodeStateData(nodeId, state, remainingSeconds);
        }
    }
}
