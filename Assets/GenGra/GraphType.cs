using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenGra
{
    public partial class GraphType : IEquatable<GraphType>
    {
        private IDictionary<string, IList<string>> _adjacencyList;

        private GraphType[] _subgraphs;

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return Equals((GraphType) obj);
            }
        }

        public override int GetHashCode()
        {
            return this.Nodes.Node.Length ^ this.Edges.Edge.Length;
        }

        public bool Equals(GraphType otherGraph)
        {
            NodeType[] thisNodes = this.Nodes.Node;
            NodeType[] otherNodes = otherGraph.Nodes.Node;

            EdgeType[] thisEdges = this.Edges.Edge;
            EdgeType[] otherEdges = otherGraph.Edges.Edge;

            if (thisNodes.Length != otherNodes.Length || thisEdges.Length != otherEdges.Length)
            {
                return false;
            }

            IDictionary<Tuple<string, int>, int> thisSymbolOutdegreeCounts = new Dictionary<Tuple<string, int>, int>();
            IDictionary<Tuple<string, int>, int> otherSymbolOutdegreeCounts = new Dictionary<Tuple<string, int>, int>();
            for (int i = 0; i < thisNodes.Length; i++)
            {
                NodeType thisNode = thisNodes[i];
                int thisAdjacentNodesCount = adjacencyList[thisNode.id].Count;
                Tuple<string, int> thisSymbolOutdegree = new Tuple<string, int>(thisNode.symbol, thisAdjacentNodesCount);
                int thisCount;
                thisSymbolOutdegreeCounts.TryGetValue(thisSymbolOutdegree, out thisCount);
                thisSymbolOutdegreeCounts[thisSymbolOutdegree] = thisCount + 1;

                NodeType otherNode = otherNodes[i];
                int otherAdjacentNodesCount = otherGraph.adjacencyList[otherNode.id].Count;
                Tuple<string, int> otherSymbolOutdegree = new Tuple<string, int>(otherNode.symbol, otherAdjacentNodesCount);
                int otherCount;
                otherSymbolOutdegreeCounts.TryGetValue(otherSymbolOutdegree, out otherCount);
                otherSymbolOutdegreeCounts[otherSymbolOutdegree] = otherCount + 1;
            }

            if (thisSymbolOutdegreeCounts.Count != otherSymbolOutdegreeCounts.Count)
            {
                return false;
            }

            foreach (Tuple<string, int> symbolOutdegree in thisSymbolOutdegreeCounts.Keys)
            {
                if (!otherSymbolOutdegreeCounts.ContainsKey(symbolOutdegree) 
                    || thisSymbolOutdegreeCounts[symbolOutdegree] != otherSymbolOutdegreeCounts[symbolOutdegree])
                {
                    return false;
                }
            }

            return true;
        }

        public GraphType[] FindMatchingSubgraphs(GraphType otherGraph)
        {
            return Array.FindAll(subgraphs, subgraph => subgraph.Equals(otherGraph));
        }

        public GraphType[] subgraphs
        {
            get
            {
                if (_subgraphs == null)
                {
                    ISet<GraphType> subgraphs = FindSubgraphs();
                    GraphType[] subgraphsArray = new GraphType[subgraphs.Count];
                    subgraphs.CopyTo(subgraphsArray, 0);
                    _subgraphs = subgraphsArray;
                }
                return _subgraphs;
            }
        }

        private ISet<GraphType> FindSubgraphs()
        {
            ISet<GraphType> subgraphs = new HashSet<GraphType>();
            foreach (NodeType node in this.Nodes.Node)
            {
                List<NodeType> nodes = new List<NodeType>();
                nodes.Add(node);

                List<EdgeType> edges = new List<EdgeType>();

                subgraphs.Add(CreateSubgraph(nodes, edges));
                
                FindAdjacentSubgraphs(node.id, subgraphs, nodes, edges);
            }
            return subgraphs;
        }

        private void FindAdjacentSubgraphs(string nodeId, ISet<GraphType> subgraphs, List<NodeType> nodes, List<EdgeType> edges)
        {
            IList<string> adjacentNodeIds = this.adjacencyList[nodeId];
            foreach (string adjacentNodeId in adjacentNodeIds)
            {
                if (edges.Exists(e => e.source == nodeId && e.target == adjacentNodeId))
                {
                    continue;
                }

                NodeType adjacentNode = this.GetNodeById(adjacentNodeId);
                nodes.Add(adjacentNode);

                EdgeType edge = new EdgeType();
                edge.source = nodeId;
                edge.target = adjacentNodeId;
                edges.Add(edge);

                subgraphs.Add(CreateSubgraph(nodes, edges));

                FindAdjacentSubgraphs(adjacentNodeId, subgraphs, nodes, edges);
            }
        }

        private NodeType GetNodeById(string id)
        {
            return Array.Find(this.Nodes.Node, n => n.id.Equals(id));
        }

        private GraphType CreateSubgraph(IList<NodeType> nodes, IList<EdgeType> edges)
        {
            GraphType subgraph = new GraphType();
            subgraph.Nodes = new NodesType();
            subgraph.Edges = new EdgesType();

            NodeType[] nodesArray = new NodeType[nodes.Count];
            nodes.CopyTo(nodesArray, 0);
            subgraph.Nodes.Node = nodesArray;

            EdgeType[] edgesArray = new EdgeType[edges.Count];
            edges.CopyTo(edgesArray, 0);
            subgraph.Edges.Edge = edgesArray;

            return subgraph;
        }

        private IDictionary<string, IList<string>> adjacencyList
        {
            get
            {
                if (this._adjacencyList == null)
                {
                    _adjacencyList = new Dictionary<string, IList<string>>();
                    foreach (NodeType node in this.Nodes.Node)
                    {
                        _adjacencyList[node.id] = new List<string>();
                    }
                    foreach (EdgeType edge in this.Edges.Edge)
                    {
                        _adjacencyList[edge.source].Add(edge.target);
                    }
                }
                return _adjacencyList;
            }
        }

    }

}