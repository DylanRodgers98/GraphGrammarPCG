using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GenGra
{
    // public partial class GraphType : IEquatable<GraphType>
    public partial class GraphType
    {
        private IDictionary<string, IList<NodeType>> adjacencyList;

        private IDictionary<string, IList<NodeType>> nodeSymbolMap;

        // private GraphType[] _subgraphs;

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

        public bool IsSupergraphOf(GraphType otherGraph)
        {
            if (!HasAllSymbolsIn(otherGraph)) return false;

            NodeType startingNode = FindStartingNode();
            
            IList<NodeType> otherAdjacentNodes = otherGraph.AdjacencyList[startingNode.id];
            IList<NodeType> nodeCandidates = NodeSymbolMap[startingNode.symbol];
            foreach (NodeType nodeCandidate in nodeCandidates)
            {
                IList<NodeType> adjacentNodes = AdjacencyList[nodeCandidate.id];
                bool isSuccessfulCandidate = Search(otherGraph, adjacentNodes, otherAdjacentNodes);
                if (isSuccessfulCandidate) return true;
            }
            return false;
        }

        private NodeType FindStartingNode()
        {
            if (Edges.Edge.Length == 0)
            {
                if (Nodes.Node.Length == 1) return Nodes.Node[0];
                throw new ArgumentException("Graph has no edges but more than one node, so cannot determine a starting node");
            }
            
            ISet<string> nodeIdsWithIndegree = new HashSet<string>();
            foreach (EdgeType edge in Edges.Edge) nodeIdsWithIndegree.Add(edge.target);

            // Return first node with an indegree of 0
            foreach (NodeType node in Nodes.Node)
            {
                if (!nodeIdsWithIndegree.Contains(node.id)) return node;
            }
            
            // If no node exists with an indegree of 0, return a random node.
            // This graph will be cyclic, so start node does not matter
            return Nodes.Node[Random.Range(0, Nodes.Node.Length - 1)];
        }

        private bool Search(GraphType otherGraph, IList<NodeType> adjacentNodes, IList<NodeType> otherAdjacentNodes, IList<string> visitedOtherNodes = null)
        {
            visitedOtherNodes = visitedOtherNodes ?? new List<string>();
            
            foreach (NodeType otherAdjacentNode in otherAdjacentNodes)
            {
                if (visitedOtherNodes.Contains(otherAdjacentNode.id)) continue;
                visitedOtherNodes.Add(otherAdjacentNode.id);
                
                bool matchingNodeFound = false;
                foreach (NodeType adjacentNode in adjacentNodes)
                {
                    if (adjacentNode.symbol == otherAdjacentNode.symbol)
                    {
                        IList<NodeType> newAdjacentNodes = AdjacencyList[adjacentNode.id];
                        IList<NodeType> newOtherAdjacentNodes = otherGraph.AdjacencyList[otherAdjacentNode.id];
                        matchingNodeFound = Search(otherGraph, newAdjacentNodes, newOtherAdjacentNodes, visitedOtherNodes);
                        if (matchingNodeFound) break;
                    }
                }
                if (!matchingNodeFound) return false;
            }

            return true;
        }

        private bool HasAllSymbolsIn(GraphType otherGraph)
        {
            return otherGraph.NodeSymbolMap.All(pair =>
            {
                string symbol = pair.Key;
                IList<NodeType> otherGraphNodes = pair.Value;
                return NodeSymbolMap.ContainsKey(symbol) && NodeSymbolMap[symbol].Count == otherGraphNodes.Count;
            });
        }

    //     public GraphType[] Subgraphs
    //     {
    //         get
    //         {
    //             if (_subgraphs == null)
    //             {
    //                 ISet<GraphType> subgraphs = FindSubgraphs();
    //                 GraphType[] subgraphsArray = new GraphType[subgraphs.Count];
    //                 subgraphs.CopyTo(subgraphsArray, 0);
    //                 _subgraphs = subgraphsArray;
    //             }
    //             return _subgraphs;
    //         }
    //     }
    //
    //     public override int GetHashCode()
    //     {
    //         return Nodes.Node.Length ^ Edges.Edge.Length;
    //     }
    //
    //     public override bool Equals(object obj)
    //     {
    //         if ((obj == null) || !GetType().Equals(obj.GetType()))
    //         {
    //             return false;
    //         }
    //         else
    //         {
    //             return Equals((GraphType) obj);
    //         }
    //     }
    //
    //     public bool Equals(GraphType otherGraph)
    //     {
    //         if (this == otherGraph)
    //         {
    //             return true;
    //         }
    //
    //         NodeType[] thisNodes = Nodes.Node;
    //         NodeType[] otherNodes = otherGraph.Nodes.Node;
    //
    //         EdgeType[] thisEdges = Edges.Edge;
    //         EdgeType[] otherEdges = otherGraph.Edges.Edge;
    //
    //         if (thisNodes.Length != otherNodes.Length || thisEdges.Length != otherEdges.Length)
    //         {
    //             return false;
    //         }
    //
    //         List<Tuple<string, int>> thisSymbolOutdegrees = new List<Tuple<string, int>>();
    //         List<Tuple<string, int>> otherSymbolOutdegrees = new List<Tuple<string, int>>();
    //
    //         for (int i = 0; i < thisNodes.Length; i++)
    //         {
    //             thisSymbolOutdegrees.Add(BuildSymbolOutdegreesTuple(thisNodes[i]));
    //             otherSymbolOutdegrees.Add(otherGraph.BuildSymbolOutdegreesTuple(otherNodes[i]));
    //         }
    //
    //         if (!thisSymbolOutdegrees.TrueForAll(o => otherSymbolOutdegrees.Contains(o)))
    //         {
    //             return false;
    //         }
    //
    //         return true;
    //     }
    //
    //     public GraphType[] FindMatchingSubgraphs(GraphType otherGraph)
    //     {
    //         return Array.FindAll(Subgraphs, subgraph => subgraph.Equals(otherGraph));
    //     }
    //
    //     private Tuple<string, int> BuildSymbolOutdegreesTuple(NodeType node)
    //     {
    //         int adjacentNodesCount = AdjacencyList[node.id].Count;
    //         return new Tuple<string, int>(node.symbol, adjacentNodesCount);
    //     }
    //
    //     private ISet<GraphType> FindSubgraphs()
    //     {
    //         ISet<GraphType> subgraphs = new HashSet<GraphType>();
    //         foreach (NodeType node in Nodes.Node)
    //         {
    //             List<NodeType> nodes = new List<NodeType>();
    //             nodes.Add(node);
    //
    //             List<EdgeType> edges = new List<EdgeType>();
    //
    //             subgraphs.Add(CreateSubgraph(nodes, edges));
    //             
    //             FindAdjacentSubgraphs(node.id, subgraphs, nodes, edges);
    //         }
    //         return subgraphs;
    //     }
    //
    //     private void FindAdjacentSubgraphs(string nodeId, ISet<GraphType> subgraphs, List<NodeType> nodes, List<EdgeType> edges)
    //     {
    //         foreach (string adjacentNodeId in AdjacencyList[nodeId])
    //         {
    //             if (edges.Exists(e => e.source == nodeId && e.target == adjacentNodeId))
    //             {
    //                 continue;
    //             }
    //
    //             NodeType adjacentNode = GetNodeById(adjacentNodeId);
    //             nodes.Add(adjacentNode);
    //
    //             EdgeType edge = new EdgeType();
    //             edge.source = nodeId;
    //             edge.target = adjacentNodeId;
    //             edges.Add(edge);
    //
    //             subgraphs.Add(CreateSubgraph(nodes, edges));
    //
    //             FindAdjacentSubgraphs(adjacentNodeId, subgraphs, nodes, edges);
    //         }
    //     }
    //
    //     private NodeType GetNodeById(string id)
    //     {
    //         return Array.Find(Nodes.Node, n => n.id.Equals(id));
    //     }
    //
    //     private GraphType CreateSubgraph(IList<NodeType> nodes, IList<EdgeType> edges)
    //     {
    //         GraphType subgraph = new GraphType();
    //         subgraph.Nodes = new NodesType();
    //         subgraph.Edges = new EdgesType();
    //
    //         NodeType[] nodesArray = new NodeType[nodes.Count];
    //         nodes.CopyTo(nodesArray, 0);
    //         subgraph.Nodes.Node = nodesArray;
    //
    //         EdgeType[] edgesArray = new EdgeType[edges.Count];
    //         edges.CopyTo(edgesArray, 0);
    //         subgraph.Edges.Edge = edgesArray;
    //
    //         return subgraph;
    //     }
    }
}