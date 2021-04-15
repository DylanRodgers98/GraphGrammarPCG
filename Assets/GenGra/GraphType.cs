using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace GenGra
{
    public partial class GraphType
    {
        private IDictionary<string, IList<NodeType>> adjacencyList;

        private IDictionary<string, IList<NodeType>> nodeSymbolMap;

        private NodeType[] startNodes;

        public IDictionary<string, IList<NodeType>> AdjacencyList
        {
            get
            {
                if (adjacencyList == null) CalculateAdjacencyList();
                return adjacencyList;
            }
        }

        public IDictionary<string, IList<NodeType>> NodeSymbolMap
        {
            get
            {
                if (nodeSymbolMap == null) CalculateNodeSymbolMap();
                return nodeSymbolMap;
            }
        }

        public NodeType[] StartNodes
        {
            get
            {
                if (startNodes == null) CalculateStartNodes();
                return startNodes;
            }
        }

        public bool IsSupergraphOf(GraphType otherGraph)
        {
            return HasAllSymbolsIn(otherGraph) && SubgraphSearch(otherGraph);
        }

        public void FindAndReplace(GraphType sourceGraph, GraphType targetGraph)
        {
            IDictionary<string, NodeType> markedNodes = FindSubgraphAndMarkNodes(sourceGraph);
            RemoveEdgesBetweenMarkedNodes(sourceGraph, markedNodes);
            FindAndReplaceNodes(sourceGraph, targetGraph, markedNodes);
            InsertEdgesBetweenMarkedNodes(targetGraph, markedNodes);
            RecalculateFields();
        }
        
        public bool HasNodesForSymbols(params string[] symbols)
        {
            return symbols.All(symbol => symbol != null && NodeSymbolMap.ContainsKey(symbol));
        }

        private void CalculateAdjacencyList()
        {
            adjacencyList = new Dictionary<string, IList<NodeType>>(Nodes.Node.Length);
            IDictionary<string, NodeType> nodes = new Dictionary<string, NodeType>(Nodes.Node.Length);

            foreach (NodeType node in Nodes.Node)
            {
                adjacencyList[node.id] = new List<NodeType>();
                nodes[node.id] = node;
            }

            if (Edges?.Edge == null) return;
            foreach (EdgeType edge in Edges.Edge)
            {
                NodeType targetNode = nodes[edge.target];
                adjacencyList[edge.source].Add(targetNode);
            }
        }

        private void CalculateNodeSymbolMap()
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

        private void CalculateStartNodes()
        {
            // If graph has no edges then nodes in graph are disconnected,
            // therefore all nodes should be used as start nodes
            if ((Edges?.Edge?.Length ?? 0) == 0)
            {
                if ((Nodes?.Node?.Length ?? 0) == 0)
                {
                    throw new InvalidOperationException(
                        "Graph has no nodes or edges. Please check the validity of your grammar");
                }

                startNodes = Nodes.Node;
                return;
            }

            // Calculate the indegree for each node in the graph
            IDictionary<string, int> nodeIndegrees = new Dictionary<string, int>();
            if (Edges?.Edge != null)
            {
                foreach (EdgeType edge in Edges.Edge)
                {
                    if (!nodeIndegrees.ContainsKey(edge.target))
                    {
                        nodeIndegrees[edge.target] = 0;
                    }

                    nodeIndegrees[edge.target]++;
                }
            }

            // Find all nodes with an indegree of 0
            NodeType[] returnNodes = Nodes.Node
                .Where(node => !nodeIndegrees.ContainsKey(node.id))
                .ToArray();

            if (returnNodes.Length != 0)
            {
                startNodes = returnNodes;
                return;
            }

            // If no node exists with an indegree of 0, then this graph is cyclic, so set startNodes
            // to be an array containing just one random start node, as the actual start node will
            // not matter since all nodes can be visited from any other node
            NodeType randomNode = Nodes.Node[Random.Range(0, Nodes.Node.Length - 1)];
            startNodes = new[] {randomNode};
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

        private bool SubgraphSearch(GraphType otherGraph, IDictionary<string, IList<NodeType>> markedNodes = null)
        {
            foreach (NodeType startNode in otherGraph.StartNodes)
            {
                IList<NodeType> sourceNodes = new List<NodeType> {startNode};
                IList<NodeType> nodeCandidates = NodeSymbolMap[startNode.symbol];
                
                bool markNodesForAllNodeCandidates = markedNodes != null && nodeCandidates.Count > 1;
                IList<IDictionary<string, IList<NodeType>>> markedNodesList = markNodesForAllNodeCandidates 
                    ? new List<IDictionary<string, IList<NodeType>>>()
                    : null;

                bool successfulCandidateFound = false;
                
                foreach (NodeType nodeCandidate in nodeCandidates)
                {
                    IList<NodeType> thisNodes = new List<NodeType> {nodeCandidate};

                    IDictionary<string, IList<NodeType>> candidateMarkedNodes = markNodesForAllNodeCandidates
                        ? new Dictionary<string, IList<NodeType>>()
                        : markedNodes;

                    bool isSuccessfulCandidate = SubgraphSearch(otherGraph, thisNodes, sourceNodes, candidateMarkedNodes);
                    if (!isSuccessfulCandidate) continue;
                    successfulCandidateFound = true;
                    
                    if (candidateMarkedNodes == null) break; // not marking nodes means we don't need to search every node, so break
                    markedNodesList?.Add(candidateMarkedNodes);
                }

                if (!successfulCandidateFound) return false;
                if (markedNodesList == null) continue;

                // pick a random collection of marked nodes to use and add contents to original markedNodes Dictionary
                IDictionary<string, IList<NodeType>> markedNodesToUse = 
                    markedNodesList[Random.Range(0, markedNodesList.Count - 1)];

                foreach (KeyValuePair<string, IList<NodeType>> keyValuePair in markedNodesToUse)
                {
                    markedNodes[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            return true;
        }

        private bool SubgraphSearch(GraphType otherGraph, IList<NodeType> thisNodes, IList<NodeType> otherNodes,
            IDictionary<string, IList<NodeType>> markedNodes = null, IList<string> visitedOtherNodes = null)
        {
            visitedOtherNodes = visitedOtherNodes ?? new List<string>(otherGraph.Nodes.Node.Length);

            foreach (NodeType otherNode in otherNodes)
            {
                if (visitedOtherNodes.Contains(otherNode.id)) continue;
                visitedOtherNodes.Add(otherNode.id);
                IList<string> visitedOtherNodesThusFar = new List<string>(visitedOtherNodes);

                bool matchingNodeFound = false;
                foreach (NodeType thisNode in thisNodes)
                {
                    if (thisNode.symbol != otherNode.symbol) continue;

                    IList<NodeType> thisAdjacentNodes = AdjacencyList[thisNode.id];
                    IList<NodeType> otherAdjacentNodes = otherGraph.AdjacencyList[otherNode.id];
                    matchingNodeFound = SubgraphSearch(otherGraph, thisAdjacentNodes,
                        otherAdjacentNodes, markedNodes, visitedOtherNodes);

                    if (!matchingNodeFound)
                    {
                        visitedOtherNodes = new List<string>(visitedOtherNodesThusFar);
                    }
                    else if (markedNodes != null)
                    {
                        if (!markedNodes.ContainsKey(otherNode.id))
                        {
                            markedNodes[otherNode.id] = new List<NodeType>();
                        }
                        markedNodes[otherNode.id].Add(thisNode);
                    }
                }

                if (!matchingNodeFound) return false;
            }

            return true;
        }

        private IDictionary<string, NodeType> FindSubgraphAndMarkNodes(GraphType otherGraph)
        {
            IDictionary<string, IList<NodeType>> candidateMarkedNodes = new Dictionary<string, IList<NodeType>>();

            bool isSupergraphOfSourceGraph = SubgraphSearch(otherGraph, candidateMarkedNodes);
            if (!isSupergraphOfSourceGraph)
            {
                throw new InvalidOperationException($"No subgraph found in graph {id} matching source graph" +
                                                    $" {otherGraph.id}, so cannot carry out find and replace operation");
            }

            IDictionary<string, NodeType> markedNodes = new Dictionary<string, NodeType>(candidateMarkedNodes.Count);
            foreach (KeyValuePair<string, IList<NodeType>> keyValuePair in candidateMarkedNodes)
            {
                IList<NodeType> nodes = keyValuePair.Value;
                NodeType nodeToUse = nodes.Count == 1 ? nodes[0] : nodes[Random.Range(0, nodes.Count - 1)];
                markedNodes[keyValuePair.Key] = nodeToUse;
            }

            return markedNodes;
        }

        private void RemoveEdgesBetweenMarkedNodes(GraphType otherGraph, IDictionary<string, NodeType> markedNodes)
        {
            if (Edges?.Edge == null || otherGraph.Edges?.Edge == null) return;
            List<EdgeType> edges = new List<EdgeType>(Edges.Edge);
            foreach (EdgeType sourceGraphEdge in otherGraph.Edges.Edge)
            {
                string sourceNodeId = markedNodes[sourceGraphEdge.source].id;
                string targetNodeId = markedNodes[sourceGraphEdge.target].id;
                edges.RemoveAll(edge => edge.source == sourceNodeId && edge.target == targetNodeId);
            }

            Edges.Edge = edges.ToArray();
        }

        /**
         * Transform this graph by transforming marked nodes into their corresponding nodes
         * in targetGraph, adding a node for each node in targetGraph that has no match in
         * this graph, and removing any nodes that have no corresponding node in targetGraph.
         */
        private void FindAndReplaceNodes(GraphType sourceGraph, GraphType targetGraph,
            IDictionary<string, NodeType> markedNodes)
        {
            List<NodeType> thisGraphNodes = new List<NodeType>(Nodes.Node);

            foreach (NodeType targetGraphNode in targetGraph.Nodes.Node)
            {
                string nodeId = targetGraphNode.id;
                string newSymbol = targetGraphNode.symbol;
                if (markedNodes.ContainsKey(nodeId))
                {
                    markedNodes[nodeId].symbol = newSymbol;
                }
                else
                {
                    List<int> numericIds = new List<int>(thisGraphNodes.Count);
                    foreach (NodeType node in thisGraphNodes)
                    {
                        if (int.TryParse(node.id, out int idAsInt))
                        {
                            numericIds.Add(idAsInt);
                        }
                    }

                    int newNodeId = 0;
                    if (numericIds.Count > 0)
                    {
                        numericIds.Sort();
                        newNodeId = numericIds.Last() + 1;
                    }

                    NodeType newNode = new NodeType
                    {
                        id = newNodeId.ToString(),
                        symbol = newSymbol
                    };

                    thisGraphNodes.Add(newNode);
                    markedNodes[nodeId] = newNode;
                }
            }

            IEnumerable<string> nodesToRemove = sourceGraph.Nodes.Node
                .Where(sourceNode => targetGraph.Nodes.Node.All(targetNode => targetNode.id != sourceNode.id))
                .Select(sourceNode => markedNodes[sourceNode.id].id);

            thisGraphNodes.RemoveAll(node => nodesToRemove.Contains(node.id));

            Nodes.Node = thisGraphNodes.ToArray();
        }

        /**
         * Copy the edges from targetGraph into this graph, determining the source and target nodes from markedNodes
         */
        private void InsertEdgesBetweenMarkedNodes(GraphType otherGraph, IDictionary<string, NodeType> markedNodes)
        {
            if (otherGraph.Edges?.Edge == null || otherGraph.Edges?.Edge?.Length == 0) return;

            IList<EdgeType> edges = Edges?.Edge != null
                ? new List<EdgeType>(Edges?.Edge)
                : new List<EdgeType>(otherGraph.Edges.Edge.Length);

            foreach (EdgeType targetGraphEdge in otherGraph.Edges.Edge)
            {
                string source = targetGraphEdge.source;
                string target = targetGraphEdge.target;

                edges.Add(new EdgeType
                {
                    source = markedNodes[source].id,
                    target = markedNodes[target].id
                });
            }

            if (Edges == null) Edges = new EdgesType();
            Edges.Edge = edges.ToArray();
        }

        private void RecalculateFields()
        {
            CalculateAdjacencyList();
            CalculateNodeSymbolMap();
            CalculateStartNodes();
        }
    }
}