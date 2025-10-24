using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Saving
{
    [Serializable]
    public class GameStateData
    {
        public List<InventoryItemData> inventory = new List<InventoryItemData>();
        public List<string> unlockedGateIds = new List<string>();
        public List<NodeStateData> nodes = new List<NodeStateData>();
        public SerializableVector3 playerPosition = SerializableVector3.Zero;
    }

    [Serializable]
    public struct InventoryItemData
    {
        public string itemId;
        public int quantity;

        public InventoryItemData(string itemId, int quantity)
        {
            this.itemId = itemId;
            this.quantity = quantity;
        }
    }

    [Serializable]
    public struct NodeStateData
    {
        public string nodeId;
        public NodeRunState state;
        public float remainingSeconds;

        public NodeStateData(string nodeId, NodeRunState state, float remainingSeconds)
        {
            this.nodeId = nodeId;
            this.state = state;
            this.remainingSeconds = remainingSeconds;
        }
    }

    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SerializableVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static SerializableVector3 Zero => new SerializableVector3(0f, 0f, 0f);
    }

    public enum NodeRunState
    {
        Inactive,
        Active,
        CoolingDown,
        Completed
    }
}
