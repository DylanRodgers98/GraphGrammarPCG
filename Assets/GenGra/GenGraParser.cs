using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace GenGra
{
    public class GenGraParser : MonoBehaviour
    {
        [SerializeField] private string missionGraphGrammarFilePath;
        [SerializeField] private GameObject buildingInstructionsFactoryPrefab;
        private BuildingInstructionsFactory buildingInstructionsFactory;

        void Start()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            long timeBeforeXmlDeserialization = stopwatch.ElapsedMilliseconds;

            GenGraType genGra;
            using (FileStream fileStream = new FileStream(missionGraphGrammarFilePath, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GenGraType));
                genGra = (GenGraType) serializer.Deserialize(fileStream);
            }

            long timeAfterXmlDeserialization = stopwatch.ElapsedMilliseconds;
            long xmlDeserializationTime = timeAfterXmlDeserialization - timeBeforeXmlDeserialization;
            Debug.Log($"XML deserialization completed in: {xmlDeserializationTime}ms");

            DebugLogDeserialization(genGra);

            long timeBeforeGraphTransformation = stopwatch.ElapsedMilliseconds;

            GraphType missionGraph = genGra.GenerateGraph();

            long timeAfterGraphTransformation = stopwatch.ElapsedMilliseconds;
            long graphTransformationTime = timeAfterGraphTransformation - timeBeforeGraphTransformation;
            Debug.Log($"Mission graph generation completed in: {graphTransformationTime}ms");
            DebugLogGraph(missionGraph);

            long timeBeforeSpaceGeneration = stopwatch.ElapsedMilliseconds;

            GenerateSpace(missionGraph);

            long timeAfterSpaceGeneration = stopwatch.ElapsedMilliseconds;
            long spaceGenerationTime = timeAfterSpaceGeneration - timeBeforeSpaceGeneration;
            Debug.Log($"Space generation completed in: {spaceGenerationTime}ms");

            stopwatch.Stop();
            Debug.Log($"Total execution completed in: {stopwatch.ElapsedMilliseconds}ms");
        }

        // TODO: remove this method when done prototyping
        private void DebugLogDeserialization(GenGraType genGra)
        {
            foreach (GraphType graph in genGra.Graphs.Graph)
            {
                DebugLogGraph(graph);
            }

            Debug.Log($"[Grammar] StartGraph: {genGra.Grammar.StartGraph.@ref}");
            RuleType[] rules = genGra.Grammar.Rules.Rule;
            for (int i = 0; i < rules.Length; i++)
            {
                RuleType rule = rules[i];
                Debug.Log($"[Grammar | Rule {i + 1}] source: {rule.source} | target: {rule.target}");
            }
        }

        // TODO: remove this method when done prototyping
        private void DebugLogGraph(GraphType graph)
        {
            foreach (NodeType node in graph.Nodes.Node)
            {
                Debug.Log($"[Graph {graph.id} | Node: {node.id}] symbol: {node.symbol}");
            }

            if (graph.Edges?.Edge == null) return;
            for (int j = 0; j < graph.Edges.Edge.Length; j++)
            {
                EdgeType edge = graph.Edges.Edge[j];
                Debug.Log($"[Graph {graph.id} | Edge: {j + 1}] source: {edge.source} | target: {edge.target}");
            }
        }

        /**
         * Carry out a BFS through missionGraph. For each node, retrieve the correct SpaceObjectByMissionSymbol based
         * on the node's symbol and place the SpaceObject on the plane of the scene, connecting it to an existing
         * SpaceObject where possible (e.g. not the first SpaceObject placed, and only connected where specified by the
         * SpaceObject - such as a doorway).
         */
        private void GenerateSpace(GraphType missionGraph)
        {
            int numStartNodes = missionGraph.StartNodes.Length;
            if (numStartNodes > 1)
            {
                throw new InvalidOperationException("Mission graph cannot have more than 1 start node. " +
                                                    $"It currently has {numStartNodes}.");
            }
            BreadthFirstSpaceGeneration(missionGraph, missionGraph.StartNodes[0]);
        }

        private void BreadthFirstSpaceGeneration(GraphType graph, NodeType startNode)
        {
            IList<string> visitedNodeIds = new List<string>(graph.Nodes.Node.Length);
            Queue<Tuple<NodeType, GameObject[]>> queue = new Queue<Tuple<NodeType, GameObject[]>>();

            visitedNodeIds.Add(startNode.id);
            GameObject[] startNodeSpaceObjects = BuildSpaceForMissionSymbol(startNode.symbol);
            queue.Enqueue(Tuple.Create(startNode, startNodeSpaceObjects));

            while (queue.Count != 0)
            {
                (NodeType currentNode, GameObject[] currentNodeSpaceObjects) = queue.Dequeue();
                IList<NodeType> adjacentNodes = graph.AdjacencyList[currentNode.id];
                foreach (NodeType adjacentNode in adjacentNodes)
                {
                    if (visitedNodeIds.Contains(adjacentNode.id)) continue;
                    visitedNodeIds.Add(adjacentNode.id);
                    GameObject[] adjacentNodeSpaceObjects = BuildSpaceForMissionSymbol(
                        adjacentNode.symbol, currentNodeSpaceObjects);
                    queue.Enqueue(Tuple.Create(adjacentNode, adjacentNodeSpaceObjects));
                }
            }
        }

        private GameObject[] BuildSpaceForMissionSymbol(string missionSymbol, GameObject[] relativeSpaceObjects = null)
        {
            if (buildingInstructionsFactory == null)
            {
                buildingInstructionsFactory = buildingInstructionsFactoryPrefab
                    .GetComponent<BuildingInstructionsFactory>();
            }

            return buildingInstructionsFactory.Build(missionSymbol, relativeSpaceObjects);
        }
    }
}