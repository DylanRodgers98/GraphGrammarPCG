using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GenGra
{
    public class LockAndKeyConnector : PostProcessor
    {
        [SerializeField] private string lockSymbol;
        [SerializeField] private string keySymbol;
        [SerializeField] private string lockMultiSymbol;
        [SerializeField] private string keyMultiSymbol;
        [SerializeField] private string lockFinalSymbol;
        [SerializeField] private string keyFinalSymbol;
        
        public override void Process(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            if (missionGraph.HasNodesForSymbols(lockSymbol, keySymbol))
            {
                ConnectKeysToLocks(missionGraph, generatedSpace);
            }
            
            if (missionGraph.HasNodesForSymbols(lockMultiSymbol, keyMultiSymbol))
            {
                ConnectMultiKeysToMultiLock(missionGraph, generatedSpace);
            }

            if (missionGraph.HasNodesForSymbols(lockFinalSymbol, keyFinalSymbol))
            {
                ConnectFinalKeyToFinalLock(missionGraph, generatedSpace);
            }
        }

        private void ConnectKeysToLocks(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            IList<NodeType> usedLockNodes = new List<NodeType>();
            IList<NodeType> suspendedKeyNodes = new List<NodeType>();
            
            IList<NodeType> keyNodes = missionGraph.NodeSymbolMap[keySymbol];
            foreach (NodeType keyNode in keyNodes)
            {
                bool isLockNodeAdjacent = false;
                IList<NodeType> adjacentNodes = missionGraph.AdjacencyList[keyNode.id];
                foreach (NodeType adjacentNode in adjacentNodes)
                {
                    if (adjacentNode.symbol == lockSymbol)
                    {
                        ConnectLockAndKey(generatedSpace, adjacentNode.id, keyNode.id);
                        usedLockNodes.Add(adjacentNode);
                        isLockNodeAdjacent = true;
                        break;
                    }
                }

                if (!isLockNodeAdjacent)
                {
                    suspendedKeyNodes.Add(keyNode);
                }
            }

            if (suspendedKeyNodes.Count == 0) return;
            
            List<NodeType> lockNodes = missionGraph.NodeSymbolMap[lockSymbol].ToList();
            lockNodes.RemoveAll(usedLockNodes.Contains);
            
            foreach (NodeType keyNode in suspendedKeyNodes)
            {
                NodeType randomLockNode = lockNodes.Count == 1
                    ? lockNodes[0] 
                    : lockNodes[Random.Range(0, lockNodes.Count)];
                
                ConnectLockAndKey(generatedSpace, randomLockNode.id, keyNode.id);
                lockNodes.Remove(randomLockNode);
            }
        }

        private void ConnectMultiKeysToMultiLock(GraphType missionGraph, 
            IDictionary<string, GameObject[]> generatedSpace)
        {
            IList<NodeType> lockMultiNodes = missionGraph.NodeSymbolMap[lockMultiSymbol];
            if (lockMultiNodes.Count != 1)
            {
                throw new InvalidOperationException("Cannot connect multi keys to multi lock due to the wrong " +
                                                    "number of lock nodes in mission graph corresponding to the lock " +
                                                    $"final symbol '{lockMultiSymbol}'. Expected 1 but found " +
                                                    $"{lockMultiNodes.Count}.");
            }
        
            string lockId = lockMultiNodes[0].id;
            GameObject[] locks = GetLocks(generatedSpace, lockId);
            if (locks.Length != 1)
            {
                throw new InvalidOperationException($"Cannot connect multi keys to multi lock (ID: {lockId}) " +
                                                    "due to the wrong number of lock GameObjects corresponding to " +
                                                    $"the lock's ID. Expected 1 but found {locks.Length}.");
            }
            
            Item[] keys = missionGraph.NodeSymbolMap[keyMultiSymbol]
                .SelectMany(node => generatedSpace[node.id])
                .SelectMany(spaceObject => spaceObject.transform.Cast<Transform>())
                .Where(child => child.CompareTag("Key"))
                .Select(keyTransform => keyTransform.GetComponent<Item>())
                .ToArray();
            
            locks[0].GetComponent<UnlockDoorAction>().AddRequiredKeys(keys);
        }

        private void ConnectFinalKeyToFinalLock(GraphType missionGraph,
            IDictionary<string, GameObject[]> generatedSpace)
        {
            IList<NodeType> lockFinalNodes = missionGraph.NodeSymbolMap[lockFinalSymbol];
            if (lockFinalNodes.Count != 1)
            {
                throw new InvalidOperationException("Cannot connect final lock to final key due to the wrong " +
                                                    "number of lock nodes in mission graph corresponding to the lock " +
                                                    $"final symbol '{lockFinalSymbol}'. Expected 1 but found " +
                                                    $"{lockFinalNodes.Count}.");
            }
            
            IList<NodeType> keyFinalNodes = missionGraph.NodeSymbolMap[keyFinalSymbol];
            if (keyFinalNodes.Count != 1)
            {
                throw new InvalidOperationException("Cannot connect final lock to final key due to the wrong " +
                                                    "number of key nodes in mission graph corresponding to the key " +
                                                    $"final symbol '{keyFinalSymbol}'. Expected 1 but found " +
                                                    $"{keyFinalNodes.Count}.");
            }
            
            ConnectLockAndKey(generatedSpace, lockFinalNodes[0].id, keyFinalNodes[0].id);
        }

        private static void ConnectLockAndKey(IDictionary<string, GameObject[]> generatedSpace, string lockId, 
            string keyId)
        {
            GameObject[] locks = GetLocks(generatedSpace, lockId);
            if (locks.Length != 1)
            {
                throw new InvalidOperationException($"Cannot connect lock (ID: {lockId}) to key (ID: {keyId}) " +
                                                    "due to the wrong number of lock GameObjects corresponding to " +
                                                    $"the lock's ID. Expected 1 but found {locks.Length}.");
            }
                    
            GameObject[] keys = GetKeys(generatedSpace, keyId);
            if (keys.Length != 1)
            {
                throw new InvalidOperationException($"Cannot connect lock (ID: {lockId}) to key (ID: {keyId}) " +
                                                    "due to the wrong number of key GameObjects corresponding to the " +
                                                    $"key's ID. Expected 1 but found {keys.Length}.");
            }

            Item key = keys[0].GetComponent<Item>();
            locks[0].GetComponent<UnlockDoorAction>().AddRequiredKeys(key);
        }

        private static GameObject[] GetLocks(IDictionary<string, GameObject[]> generatedSpace, string nodeId)
        {
            return GetChildrenByTag(generatedSpace, nodeId, "Lock");
        }

        private static GameObject[] GetKeys(IDictionary<string, GameObject[]> generatedSpace, string nodeId)
        {
            return GetChildrenByTag(generatedSpace, nodeId, "Key");
        }

        private static GameObject[] GetChildrenByTag(IDictionary<string, GameObject[]> generatedSpace,
            string nodeId, string tag)
        {
            return generatedSpace[nodeId]
                .SelectMany(spaceObject => spaceObject.transform.Cast<Transform>())
                .Where(child => child.CompareTag(tag))
                .Select(transform => transform.gameObject)
                .ToArray();
        }
    }
}