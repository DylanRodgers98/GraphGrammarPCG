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

        private IDictionary<string, IList<string>> AdjacencyList
        {
            get
            {
                if (_adjacencyList == null)
                {
                    _adjacencyList = new Dictionary<string, IList<string>>();
                    foreach (NodeType node in Nodes.Node)
                    {
                        _adjacencyList[node.id] = new List<string>();
                    }
                    foreach (EdgeType edge in Edges.Edge)
                    {
                        _adjacencyList[edge.source].Add(edge.target);
                    }
                }
                return _adjacencyList;
            }
        }

        public GraphType[] Subgraphs
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

        public override int GetHashCode()
        {
            return Nodes.Node.Length ^ Edges.Edge.Length;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return Equals((GraphType) obj);
            }
        }

        public bool Equals(GraphType otherGraph)
        {
            if (this == otherGraph)
            {
                return true;
            }

            NodeType[] thisNodes = Nodes.Node;
            NodeType[] otherNodes = otherGraph.Nodes.Node;

            EdgeType[] thisEdges = Edges.Edge;
            EdgeType[] otherEdges = otherGraph.Edges.Edge;

            if (thisNodes.Length != otherNodes.Length || thisEdges.Length != otherEdges.Length)
            {
                return false;
            }

            List<Tuple<string, int>> thisSymbolOutdegrees = new List<Tuple<string, int>>();
            List<Tuple<string, int>> otherSymbolOutdegrees = new List<Tuple<string, int>>();

            for (int i = 0; i < thisNodes.Length; i++)
            {
                thisSymbolOutdegrees.Add(BuildSymbolOutdegreesTuple(thisNodes[i]));
                otherSymbolOutdegrees.Add(otherGraph.BuildSymbolOutdegreesTuple(otherNodes[i]));
            }

            if (!thisSymbolOutdegrees.TrueForAll(o => otherSymbolOutdegrees.Contains(o)))
            {
                return false;
            }

            return true;
        }

        public GraphType[] FindMatchingSubgraphs(GraphType otherGraph)
        {
            return Array.FindAll(Subgraphs, subgraph => subgraph.Equals(otherGraph));
        }

        private Tuple<string, int> BuildSymbolOutdegreesTuple(NodeType node)
        {
            int adjacentNodesCount = AdjacencyList[node.id].Count;
            return new Tuple<string, int>(node.symbol, adjacentNodesCount);
        }

        private ISet<GraphType> FindSubgraphs()
        {
            ISet<GraphType> subgraphs = new HashSet<GraphType>();
            foreach (NodeType node in Nodes.Node)
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
            foreach (string adjacentNodeId in AdjacencyList[nodeId])
            {
                if (edges.Exists(e => e.source == nodeId && e.target == adjacentNodeId))
                {
                    continue;
                }

                NodeType adjacentNode = GetNodeById(adjacentNodeId);
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
            return Array.Find(Nodes.Node, n => n.id.Equals(id));
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
    }
}