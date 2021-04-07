using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GenGra
{
    public partial class GraphType
    {
        private IDictionary<string, IList<NodeType>> adjacencyList;

        private IDictionary<string, IList<NodeType>> nodeSymbolMap;

        private NodeType[] startingNodes;

        public IDictionary<string, IList<NodeType>> AdjacencyList
        {
            get
            {
                if (adjacencyList == null)
                {
                    adjacencyList = new Dictionary<string, IList<NodeType>>();
                    IDictionary<string, NodeType> nodes = new Dictionary<string, NodeType>();
                    
                    foreach (NodeType node in Nodes.Node)
                    {
                        adjacencyList[node.id] = new List<NodeType>();
                        nodes[node.id] = node;
                    }
                    foreach (EdgeType edge in Edges.Edge)
                    {
                        NodeType targetNode = nodes[edge.target];
                        adjacencyList[edge.source].Add(targetNode);
                    }
                }
                
                return adjacencyList;
            }
        }

        public IDictionary<string, IList<NodeType>> NodeSymbolMap
        {
            get
            {
                if (nodeSymbolMap == null)
                {
                    nodeSymbolMap = new Dictionary<string, IList<NodeType>>();
                    foreach (NodeType node in Nodes.Node)
                    {
                        if (!nodeSymbolMap.ContainsKey(node.symbol))
                        {
                            nodeSymbolMap[node.symbol] = new List<NodeType>();
                        }
                        nodeSymbolMap[node.symbol].Add(node);
                    }
                }

                return nodeSymbolMap;
            }
        }

        public NodeType[] StartingNodes
        {
            get
            {
                if (startingNodes != null) return startingNodes;
                
                // If graph has no edges then nodes in graph are disconnected,
                // therefore all nodes should be used as starting nodes
                if (Edges.Edge.Length == 0)
                {
                    if (Nodes.Node.Length == 0)
                    {
                        throw new InvalidOperationException(
                            "Graph has no nodes or edges. Please check the validity of your grammar");
                    }
                    startingNodes = Nodes.Node;
                    return startingNodes;
                }

                // Calculate the indegree for each node in the graph
                IDictionary<string, int> nodeIndegrees = new Dictionary<string, int>();
                foreach (EdgeType edge in Edges.Edge)
                {
                    if (!nodeIndegrees.ContainsKey(edge.target))
                    {
                        nodeIndegrees[edge.target] = 0;
                    }
                    nodeIndegrees[edge.target]++;
                }

                // Find all nodes with an indegree of 0
                NodeType[] returnNodes = Nodes.Node
                    .Where(node => !nodeIndegrees.ContainsKey(node.id))
                    .ToArray();

                if (returnNodes.Length != 0)
                {
                    startingNodes = returnNodes;
                    return startingNodes;
                }

                // If no node exists with an indegree of 0, then this graph is cyclic, so set startingNodes
                // to be an array containing just one random start node, as the actual start node will not
                // matter since all nodes can be visited from any other node
                NodeType randomNode = Nodes.Node[Random.Range(0, Nodes.Node.Length - 1)];
                startingNodes = new [] {randomNode};
                return startingNodes;
            }
        }

        public bool IsSupergraphOf(GraphType otherGraph)
        {
            return HasAllSymbolsIn(otherGraph) && DualSearch(otherGraph);
        }
        
        public void FindAndReplace(GraphType sourceGraph, GraphType targetGraph)
        {
            /*
             * STEP 1: Find a subgraph in the target graph that matches the left-hand part
             * of the rule and mark that subgraph by copying the identifiers of the nodes. 
             */
            
            IDictionary<string, NodeType> nodesMarkedBySourceNodeId = new Dictionary<string, NodeType>();
            bool isSupergraphOfSourceGraph = DualSearch(sourceGraph, nodesMarkedBySourceNodeId);
            if (!isSupergraphOfSourceGraph)
            {
                throw new InvalidOperationException($"No subgraph found in graph {id} matching source graph" +
                                                    $" {sourceGraph.id}, so cannot carry out find and replace operation");
            }

            /*
             * STEP 2: Remove all edges between the marked nodes. 
             */
            
            List<EdgeType> thisGraphEdges = new List<EdgeType>(Edges.Edge);
            
            foreach (EdgeType sourceGraphEdge in sourceGraph.Edges.Edge)
            {
                string sourceNodeId = nodesMarkedBySourceNodeId[sourceGraphEdge.source].id;
                string targetNodeId = nodesMarkedBySourceNodeId[sourceGraphEdge.target].id;
                thisGraphEdges.RemoveAll(edge => edge.source == sourceNodeId && edge.target == targetNodeId);
            }
            
            /*
             * STEP 3: Transform the graph by transforming marked nodes into their corresponding nodes on the
             * right-hand side, adding a node for each node on the right-hand side that has no match in the
             * target graph, and removing any nodes that have no corresponding node on the right-hand side.
             */

            List<NodeType> thisGraphNodes = new List<NodeType>(Nodes.Node);
            
            IEnumerable<string> nodesToRemove = sourceGraph.Nodes.Node
                .Where(sourceNode => targetGraph.Nodes.Node.All(targetNode => targetNode.id != sourceNode.id))
                .Select(sourceNode => nodesMarkedBySourceNodeId[sourceNode.id].id);
            
            thisGraphNodes.RemoveAll(node => nodesToRemove.Contains(node.id));

            foreach (NodeType targetGraphNode in targetGraph.Nodes.Node)
            {
                string nodeId = targetGraphNode.id;
                string newSymbol = targetGraphNode.symbol;
                if (nodesMarkedBySourceNodeId.ContainsKey(nodeId))
                {
                    nodesMarkedBySourceNodeId[nodeId].symbol = newSymbol;
                }
                else
                {
                    int newNodeId = thisGraphNodes
                        .Select(node => int.Parse(node.id))
                        .OrderBy(i => i)
                        .Last() + 1;

                    NodeType newNode = new NodeType
                    {
                        id = newNodeId.ToString(),
                        symbol = newSymbol
                    };
                    
                    thisGraphNodes.Add(newNode);
                    nodesMarkedBySourceNodeId[nodeId] = newNode;
                }
            }

            Nodes.Node = thisGraphNodes.ToArray();
            
            /*
             * STEP 4: Copy the edges as specified by the right-hand side.
             */

            foreach (EdgeType targetGraphEdge in targetGraph.Edges.Edge)
            {
                string source = targetGraphEdge.source;
                string target = targetGraphEdge.target;
                
                thisGraphEdges.Add(new EdgeType
                {
                    source = nodesMarkedBySourceNodeId[source].id,
                    target = nodesMarkedBySourceNodeId[target].id
                });
            }

            Edges.Edge = thisGraphEdges.ToArray();
        }

        private bool HasAllSymbolsIn(GraphType otherGraph)
        {
            return otherGraph.NodeSymbolMap.All(pair =>
            {
                string symbol = pair.Key;
                IList<NodeType> otherGraphNodes = pair.Value;
                return NodeSymbolMap.ContainsKey(symbol) && 
                       NodeSymbolMap[symbol].Count >= otherGraphNodes.Count;
            });
        }
        
        private bool DualSearch(GraphType otherGraph, IDictionary<string, NodeType> nodesMarkedByOtherNodeId = null)
        {
            foreach (NodeType startingNode in otherGraph.StartingNodes)
            {
                IList<NodeType> sourceNodes = new List<NodeType>(otherGraph.AdjacencyList[startingNode.id]);
                sourceNodes.Add(startingNode);

                bool isSuccessfulCandidate = false;
                IList<NodeType> nodeCandidates = NodeSymbolMap[startingNode.symbol];
                foreach (NodeType nodeCandidate in nodeCandidates)
                {
                    IList<NodeType> thisNodes = new List<NodeType>(AdjacencyList[nodeCandidate.id]);
                    thisNodes.Add(nodeCandidate);

                    isSuccessfulCandidate = DualSearch(otherGraph, thisNodes, sourceNodes, nodesMarkedByOtherNodeId);
                    if (isSuccessfulCandidate) break;
                    if (nodesMarkedByOtherNodeId != null)
                    {
                        nodesMarkedByOtherNodeId = new Dictionary<string, NodeType>();
                    }
                }
                if (!isSuccessfulCandidate) return false;
            }

            return true;
        }
        
        private bool DualSearch(GraphType otherGraph, IList<NodeType> thisNodes, IList<NodeType> otherNodes,
            IDictionary<string, NodeType> nodesMarkedByOtherNodeId = null)
        {
            foreach (NodeType otherNode in otherNodes)
            {
                bool matchingNodeFound = false;
                foreach (NodeType thisNode in thisNodes)
                {
                    if (thisNode.symbol != otherNode.symbol) continue;
                    
                    IList<NodeType> thisAdjacentNodes = AdjacencyList[thisNode.id];
                    IList<NodeType> otherAdjacentNodes = otherGraph.AdjacencyList[otherNode.id];
                    matchingNodeFound = DualSearch(otherGraph, thisAdjacentNodes, otherAdjacentNodes, nodesMarkedByOtherNodeId);
                    if (matchingNodeFound && nodesMarkedByOtherNodeId != null)
                    {
                        nodesMarkedByOtherNodeId[otherNode.id] = thisNode;
                    }
                }
                if (!matchingNodeFound) return false;
            }

            return true;
        }
    }
}