using System;
using UnityEngine;

namespace Game.Saving
{
    public class GateState : MonoBehaviour
    {
        [SerializeField]
        private string gateId;

        [SerializeField]
        private bool unlocked;

        public string GateId => gateId;

        public bool IsUnlocked => unlocked;

        public event Action<GateState> GateStateChanged;

        private void OnEnable()
        {
            GameSaveManager.Instance?.RegisterGate(this);
        }

        private void OnDisable()
        {
            GameSaveManager.Instance?.UnregisterGate(this);
        }

        public void Unlock()
        {
            SetUnlocked(true);
        }

        public void Lock()
        {
            SetUnlocked(false);
        }

        public void SetUnlocked(bool value)
        {
            SetUnlocked(value, false);
        }

        public void SetUnlocked(bool value, bool silent)
        {
            if (unlocked == value)
            {
                if (!silent)
                {
                    GateStateChanged?.Invoke(this);
                }

                return;
            }

            unlocked = value;
            if (!silent)
            {
                GateStateChanged?.Invoke(this);
            }
        }
    }
}
