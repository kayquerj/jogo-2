using System;
using System.Collections.Generic;
using System.IO;
using Game.Mobile;
using UnityEngine;

namespace Game.Saving
{
    public class GameSaveManager : MonoBehaviour
    {
        private const string SaveFileName = "game_state.json";

        [Header("Tracked Systems")]
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private List<GateState> gates = new List<GateState>();
        [SerializeField] private List<ResourceNodeState> resourceNodes = new List<ResourceNodeState>();

        [Header("Behaviour")]
        [SerializeField] private bool autoDiscoverParticipants = true;
        [SerializeField] private bool persistAcrossScenes = true;

        private readonly HashSet<GateState> gateSet = new HashSet<GateState>();
        private readonly HashSet<ResourceNodeState> nodeSet = new HashSet<ResourceNodeState>();

        private string saveFilePath = string.Empty;
        private bool suppressAutoSaves;
        private bool isInitialising = true;
        private GameStateData cachedState = new GameStateData();

        public static GameSaveManager Instance { get; private set; }
        public string SaveFilePath => saveFilePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            saveFilePath = Path.Combine(Application.persistentDataPath, SaveFileName);

            NormaliseTrackedLists();

            if (autoDiscoverParticipants)
            {
                DiscoverParticipants();
            }

            cachedState = BuildCurrentState();
            isInitialising = false;

            LoadGame();
        }

        private void OnEnable()
        {
            SubscribeAll();
        }

        private void Start()
        {
            if (autoDiscoverParticipants)
            {
                if (!inventorySystem)
                {
                    var discoveredInventory = FindObjectOfType<InventorySystem>(true);
                    if (discoveredInventory)
                    {
                        RegisterInventorySystem(discoveredInventory);
                    }
                }

                if (!playerTransform)
                {
                    playerTransform = FindPlayerTransform();
                }

                foreach (var gate in FindObjectsOfType<GateState>(true))
                {
                    RegisterGate(gate);
                }

                foreach (var node in FindObjectsOfType<ResourceNodeState>(true))
                {
                    RegisterResourceNode(node);
                }
            }

            SubscribeAll();
            ApplyCachedPlayerPosition();
        }

        private void OnDisable()
        {
            UnsubscribeAll();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterInventorySystem(InventorySystem system)
        {
            if (!system || inventorySystem == system)
            {
                return;
            }

            if (inventorySystem && isActiveAndEnabled)
            {
                inventorySystem.InventoryChanged -= HandleInventoryChanged;
            }

            inventorySystem = system;

            if (!isInitialising && cachedState != null)
            {
                inventorySystem.RestoreState(cachedState.inventory, true);
            }

            if (isActiveAndEnabled)
            {
                inventorySystem.InventoryChanged += HandleInventoryChanged;
            }
        }

        public void UnregisterInventorySystem(InventorySystem system)
        {
            if (inventorySystem != system)
            {
                return;
            }

            if (isActiveAndEnabled)
            {
                inventorySystem.InventoryChanged -= HandleInventoryChanged;
            }

            inventorySystem = null;
        }

        public void RegisterPlayerTransform(Transform player)
        {
            if (!player)
            {
                return;
            }

            playerTransform = player;

            if (!isInitialising)
            {
                ApplyCachedPlayerPosition();
            }
        }

        public void UnregisterPlayerTransform(Transform player)
        {
            if (playerTransform == player)
            {
                playerTransform = null;
            }
        }

        public void RegisterGate(GateState gate)
        {
            if (!gate || gateSet.Contains(gate))
            {
                return;
            }

            gateSet.Add(gate);
            gates.Add(gate);

            if (!isInitialising)
            {
                ApplyCachedStateToGate(gate);
            }

            if (isActiveAndEnabled)
            {
                gate.GateStateChanged += HandleGateStateChanged;
            }
        }

        public void UnregisterGate(GateState gate)
        {
            if (!gateSet.Remove(gate))
            {
                return;
            }

            if (isActiveAndEnabled)
            {
                gate.GateStateChanged -= HandleGateStateChanged;
            }

            gates.Remove(gate);
        }

        public void RegisterResourceNode(ResourceNodeState node)
        {
            if (!node || nodeSet.Contains(node))
            {
                return;
            }

            nodeSet.Add(node);
            resourceNodes.Add(node);

            if (!isInitialising)
            {
                ApplyCachedStateToNode(node);
            }

            if (isActiveAndEnabled)
            {
                node.NodeStateChanged += HandleNodeStateChanged;
            }
        }

        public void UnregisterResourceNode(ResourceNodeState node)
        {
            if (!nodeSet.Remove(node))
            {
                return;
            }

            if (isActiveAndEnabled)
            {
                node.NodeStateChanged -= HandleNodeStateChanged;
            }

            resourceNodes.Remove(node);
        }

        public void SaveGame(bool force = false)
        {
            if (!force && suppressAutoSaves)
            {
                return;
            }

            var state = BuildCurrentState();
            cachedState = CloneState(state);

            string json;
            try
            {
                json = JsonUtility.ToJson(state, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to serialise game state: {ex}");
                return;
            }

            try
            {
                var directory = Path.GetDirectoryName(saveFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(saveFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write game state to '{saveFilePath}': {ex}");
            }
        }

        public void LoadGame()
        {
            var data = TryReadSaveData(out bool hadCorruption);
            if (data != null)
            {
                ApplyState(data);
            }
            else if (hadCorruption)
            {
                SaveGame(force: true);
            }
        }

        public void DeleteSaveData()
        {
            if (!File.Exists(saveFilePath))
            {
                return;
            }

            try
            {
                File.Delete(saveFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to delete save file '{saveFilePath}': {ex}");
            }
        }

        private void SubscribeAll()
        {
            UnsubscribeAll();

            if (inventorySystem)
            {
                inventorySystem.InventoryChanged += HandleInventoryChanged;
            }

            foreach (var gate in gateSet)
            {
                if (gate)
                {
                    gate.GateStateChanged += HandleGateStateChanged;
                }
            }

            foreach (var node in nodeSet)
            {
                if (node)
                {
                    node.NodeStateChanged += HandleNodeStateChanged;
                }
            }
        }

        private void UnsubscribeAll()
        {
            if (inventorySystem)
            {
                inventorySystem.InventoryChanged -= HandleInventoryChanged;
            }

            foreach (var gate in gateSet)
            {
                if (gate)
                {
                    gate.GateStateChanged -= HandleGateStateChanged;
                }
            }

            foreach (var node in nodeSet)
            {
                if (node)
                {
                    node.NodeStateChanged -= HandleNodeStateChanged;
                }
            }
        }

        private void DiscoverParticipants()
        {
            if (!inventorySystem)
            {
                var discoveredInventory = FindObjectOfType<InventorySystem>(true);
                if (discoveredInventory)
                {
                    RegisterInventorySystem(discoveredInventory);
                }
            }

            if (!playerTransform)
            {
                playerTransform = FindPlayerTransform();
            }

            foreach (var gate in FindObjectsOfType<GateState>(true))
            {
                RegisterGate(gate);
            }

            foreach (var node in FindObjectsOfType<ResourceNodeState>(true))
            {
                RegisterResourceNode(node);
            }
        }

        private void NormaliseTrackedLists()
        {
            gates.RemoveAll(g => g == null);
            resourceNodes.RemoveAll(n => n == null);

            gateSet.Clear();
            var uniqueGates = new List<GateState>(gates.Count);
            foreach (var gate in gates)
            {
                if (gateSet.Add(gate))
                {
                    uniqueGates.Add(gate);
                }
            }

            if (uniqueGates.Count != gates.Count)
            {
                gates.Clear();
                gates.AddRange(uniqueGates);
            }

            nodeSet.Clear();
            var uniqueNodes = new List<ResourceNodeState>(resourceNodes.Count);
            foreach (var node in resourceNodes)
            {
                if (nodeSet.Add(node))
                {
                    uniqueNodes.Add(node);
                }
            }

            if (uniqueNodes.Count != resourceNodes.Count)
            {
                resourceNodes.Clear();
                resourceNodes.AddRange(uniqueNodes);
            }
        }

        private InventorySystem FindInventorySystem()
        {
            var systems = FindObjectsOfType<InventorySystem>(true);
            return systems.Length > 0 ? systems[0] : null;
        }

        private Transform FindPlayerTransform()
        {
            var mobileControllers = FindObjectsOfType<MobilePlayerController>(true);
            if (mobileControllers.Length > 0)
            {
                return mobileControllers[0].transform;
            }

            var characterControllers = FindObjectsOfType<CharacterController>(true);
            return characterControllers.Length > 0 ? characterControllers[0].transform : null;
        }

        private GameStateData BuildCurrentState()
        {
            var state = new GameStateData
            {
                inventory = inventorySystem ? inventorySystem.CaptureState() : new List<InventoryItemData>(),
                unlockedGateIds = new List<string>(),
                nodes = new List<NodeStateData>(),
                playerPosition = playerTransform ? new SerializableVector3(playerTransform.position) : SerializableVector3.Zero
            };

            var seenGateIds = new HashSet<string>();
            foreach (var gate in gates)
            {
                if (!gate || string.IsNullOrEmpty(gate.GateId) || !gate.IsUnlocked)
                {
                    continue;
                }

                if (seenGateIds.Add(gate.GateId))
                {
                    state.unlockedGateIds.Add(gate.GateId);
                }
            }

            var seenNodeIds = new HashSet<string>();
            foreach (var node in resourceNodes)
            {
                if (!node || string.IsNullOrEmpty(node.NodeId) || !seenNodeIds.Add(node.NodeId))
                {
                    continue;
                }

                state.nodes.Add(node.ToSnapshot());
            }

            return state;
        }

        private void ApplyState(GameStateData data)
        {
            if (data == null)
            {
                return;
            }

            EnsureCollections(data);
            cachedState = CloneState(data);

            suppressAutoSaves = true;
            try
            {
                if (inventorySystem)
                {
                    inventorySystem.RestoreState(data.inventory, true);
                }

                var unlocked = new HashSet<string>(data.unlockedGateIds);
                foreach (var gate in gates)
                {
                    if (!gate || string.IsNullOrEmpty(gate.GateId))
                    {
                        continue;
                    }

                    gate.SetUnlocked(unlocked.Contains(gate.GateId), true);
                }

                var lookup = BuildNodeLookup(data.nodes);
                foreach (var node in resourceNodes)
                {
                    if (!node || string.IsNullOrEmpty(node.NodeId))
                    {
                        continue;
                    }

                    if (lookup.TryGetValue(node.NodeId, out var snapshot))
                    {
                        node.ApplySnapshot(snapshot, true);
                    }
                }

                ApplyCachedPlayerPosition();
            }
            finally
            {
                suppressAutoSaves = false;
            }
        }

        private void ApplyCachedStateToGate(GateState gate)
        {
            if (!gate || cachedState == null || string.IsNullOrEmpty(gate.GateId))
            {
                return;
            }

            if (cachedState.unlockedGateIds != null && cachedState.unlockedGateIds.Contains(gate.GateId))
            {
                gate.SetUnlocked(true, true);
            }
            else
            {
                gate.SetUnlocked(false, true);
            }
        }

        private void ApplyCachedStateToNode(ResourceNodeState node)
        {
            if (!node || cachedState == null || string.IsNullOrEmpty(node.NodeId) || cachedState.nodes == null)
            {
                return;
            }

            foreach (var snapshot in cachedState.nodes)
            {
                if (snapshot.nodeId == node.NodeId)
                {
                    node.ApplySnapshot(snapshot, true);
                    return;
                }
            }
        }

        private void ApplyCachedPlayerPosition()
        {
            if (!playerTransform || cachedState == null)
            {
                return;
            }

            Vector3 position = cachedState.playerPosition.ToVector3();
            var mobileController = playerTransform.GetComponent<MobilePlayerController>();
            if (mobileController)
            {
                mobileController.TeleportTo(position);
            }
            else
            {
                playerTransform.position = position;
            }
        }

        private static Dictionary<string, NodeStateData> BuildNodeLookup(IEnumerable<NodeStateData> nodes)
        {
            var result = new Dictionary<string, NodeStateData>();
            if (nodes == null)
            {
                return result;
            }

            foreach (var node in nodes)
            {
                if (string.IsNullOrEmpty(node.nodeId))
                {
                    continue;
                }

                result[node.nodeId] = node;
            }

            return result;
        }

        private GameStateData TryReadSaveData(out bool hadCorruption)
        {
            hadCorruption = false;

            if (!File.Exists(saveFilePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(saveFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    hadCorruption = true;
                    return null;
                }

                var data = JsonUtility.FromJson<GameStateData>(json);
                if (data == null)
                {
                    hadCorruption = true;
                    return null;
                }

                EnsureCollections(data);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load save data from '{saveFilePath}': {ex}");
                hadCorruption = true;
                return null;
            }
        }

        private static void EnsureCollections(GameStateData data)
        {
            if (data.inventory == null)
            {
                data.inventory = new List<InventoryItemData>();
            }

            if (data.unlockedGateIds == null)
            {
                data.unlockedGateIds = new List<string>();
            }

            if (data.nodes == null)
            {
                data.nodes = new List<NodeStateData>();
            }
        }

        private static GameStateData CloneState(GameStateData source)
        {
            if (source == null)
            {
                return null;
            }

            EnsureCollections(source);
            return new GameStateData
            {
                inventory = new List<InventoryItemData>(source.inventory),
                unlockedGateIds = new List<string>(source.unlockedGateIds),
                nodes = new List<NodeStateData>(source.nodes),
                playerPosition = source.playerPosition
            };
        }

        private void HandleInventoryChanged()
        {
            RequestAutoSave();
        }

        private void HandleGateStateChanged(GateState _)
        {
            RequestAutoSave();
        }

        private void HandleNodeStateChanged(ResourceNodeState _)
        {
            RequestAutoSave();
        }

        private void RequestAutoSave()
        {
            if (!suppressAutoSaves)
            {
                SaveGame();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame(force: true);
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame(force: true);
        }
    }
}
