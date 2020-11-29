using System;
using System.Collections.Generic;

namespace GenGra
{
    public partial class GraphType
    {

        private IDictionary<string, IList<string>> _adjacencyList;

        public bool ContainsSubgraph(GraphType otherGraph)
        {
            if (this == otherGraph)
            {
                return true;
            }

            foreach (string otherNodeId in otherGraph.adjacencyList.Keys)
            {
                IDictionary<string, int> otherAdjacentNodeSymbolCounts = otherGraph.GetAdjacentNodeSymbolCounts(otherNodeId);

                // get nodes from this graph with symbol matching that of node from otherGraph
                NodeType otherNode = otherGraph.GetNodeById(otherNodeId);
                NodeType[] thisNodesMatchingSymbol = this.GetNodesBySymbol(otherNode.symbol);

                // if nodes with matching symbols exists, and node from otherGraph has no adjacent nodes, then continue
                if (thisNodesMatchingSymbol.Length > 0 && otherAdjacentNodeSymbolCounts.Keys.Count == 0)
                {
                    continue;
                }

                bool matchingNodeFound = false;

                foreach (NodeType thisNode in thisNodesMatchingSymbol)
                {
                    IDictionary<string, int> thisAdjacentNodeSymbolCounts = this.GetAdjacentNodeSymbolCounts(thisNode.id);

                    // check to see if list of adjacent symbols from otherGraph is a sublist of list of adjacent symbols for current node from this graph
                    bool isAdjacencyListSublist = true;
                    foreach (string otherAdjacentNodeSymbol in otherAdjacentNodeSymbolCounts.Keys)
                    {
                        if (!thisAdjacentNodeSymbolCounts.ContainsKey(otherAdjacentNodeSymbol)
                            || thisAdjacentNodeSymbolCounts[otherAdjacentNodeSymbol] < otherAdjacentNodeSymbolCounts[otherAdjacentNodeSymbol])
                        {
                            isAdjacencyListSublist = false;
                            break;
                        }
                    }
                    if (isAdjacencyListSublist)
                    {
                        matchingNodeFound = true;
                        break;
                    }
                }

                if (!matchingNodeFound)
                {
                    return false;
                }
            }
            return true;
        }

        private IDictionary<string, IList<string>> adjacencyList
        {
            get
            {
                if (_adjacencyList == null)
                {
                    IDictionary<string, IList<string>> adjacencyList = new Dictionary<string, IList<string>>();
                    foreach (EdgeType edge in this.Edges.Edge)
                    {
                        if (!adjacencyList.ContainsKey(edge.source))
                        {
                            adjacencyList[edge.source] = new List<string>();
                        }
                        adjacencyList[edge.source].Add(edge.target);
                    }
                    _adjacencyList = adjacencyList;
                }
                return _adjacencyList;
            }
        }

        private IDictionary<string, int> GetAdjacentNodeSymbolCounts(string nodeId)
        {
            IDictionary<string, int> adjacentNodeSymbols = new Dictionary<string, int>();
            foreach (string adjecentNodeId in this.adjacencyList[nodeId])
            {
                NodeType adjacentNode = this.GetNodeById(adjecentNodeId);
                if (!adjacentNodeSymbols.ContainsKey(adjacentNode.symbol))
                {
                    adjacentNodeSymbols[adjacentNode.symbol] = 1;
                }
                else
                {
                    adjacentNodeSymbols[adjacentNode.symbol]++;
                }
            }
            return adjacentNodeSymbols;
        }

        private NodeType GetNodeById(string id)
        {
            return Array.Find(this.Nodes.Node, n => n.id.Equals(id));
        }

        private NodeType[] GetNodesBySymbol(string symbol)
        {
            return Array.FindAll(this.Nodes.Node, n => n.symbol.Equals(symbol));
        }

    }

}