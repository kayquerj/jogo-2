using System.Collections;
using Harvesting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ResourceNodePlayModeTests
{
    [UnityTest]
    public IEnumerator ResourceNode_HarvestCompletesAndRespawns()
    {
        var nodeGameObject = new GameObject("ResourceNode_Test");
        nodeGameObject.transform.position = Vector3.zero;
        var nodeCollider = nodeGameObject.AddComponent<SphereCollider>();
        nodeCollider.radius = 1.5f;
        nodeCollider.isTrigger = true;

        var resourceNode = nodeGameObject.AddComponent<ResourceNode>();

        var config = ScriptableObject.CreateInstance<TreeResourceNodeConfig>();
        config.Configure("Tree", 0.3f, 0.4f, new[] { new ResourceYield("wood", 2) });
        resourceNode.Config = config;

        var harvesterGameObject = new GameObject("Harvester_Test");
        harvesterGameObject.transform.position = new Vector3(2f, 0f, 0f);
        var rb = harvesterGameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        var harvesterCollider = harvesterGameObject.AddComponent<SphereCollider>();
        harvesterCollider.radius = 0.5f;
        harvesterCollider.isTrigger = false;

        var harvester = harvesterGameObject.AddComponent<HarvestAgentStub>();

        yield return null;

        harvesterGameObject.transform.position = Vector3.zero;

        yield return null;

        Assert.AreEqual(ResourceNodeState.Harvesting, resourceNode.State);

        yield return new WaitUntil(() => resourceNode.State == ResourceNodeState.CoolingDown);

        Assert.AreEqual(2, harvester.TotalCollected);

        harvesterGameObject.transform.position = new Vector3(3f, 0f, 0f);
        yield return null;

        yield return new WaitUntil(() => resourceNode.State == ResourceNodeState.Available);
        Assert.AreEqual(ResourceNodeState.Available, resourceNode.State);

        harvesterGameObject.transform.position = Vector3.zero;
        yield return null;

        Assert.AreEqual(ResourceNodeState.Harvesting, resourceNode.State);

        Object.Destroy(nodeGameObject);
        Object.Destroy(harvesterGameObject);
        Object.Destroy(config);
        yield return null;
    }

    private class HarvestAgentStub : MonoBehaviour, IHarvestAgent
    {
        public int TotalCollected { get; private set; }

        public void ReceiveResources(ResourceYield[] yields)
        {
            if (yields == null)
            {
                return;
            }

            foreach (var yield in yields)
            {
                TotalCollected += yield.amount;
            }
        }
    }
}
