using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GenGra
{
    public class GenGraGenerator : MonoBehaviour
    {
        [SerializeField] private string missionGraphGrammarFilePath;
        
        /*
         * See doc comment on struct BuildingInstructionsHolder about
         * the necessity for both a public Array and a private IDictionary
         */
        [SerializeField] private BuildingInstructionsHolder[] buildingInstructionsByMissionSymbol;
        private IDictionary<string, GameObject> buildingInstructionsByMissionSymbolDict;

        private IDictionary<string, GameObject> BuildingInstructionsByMissionSymbol
        {
            get
            {
                if (buildingInstructionsByMissionSymbolDict == null)
                {
                    buildingInstructionsByMissionSymbolDict = new Dictionary<string, GameObject>();
                    foreach (BuildingInstructionsHolder biHolder in buildingInstructionsByMissionSymbol)
                    {
                        buildingInstructionsByMissionSymbolDict[biHolder.MissionSymbol] =
                            biHolder.BuildingInstructionsPrefab;
                    }
                }

                return buildingInstructionsByMissionSymbolDict;
            }
        }

        private void Start()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            long timeBeforeXmlDeserialization = stopwatch.ElapsedMilliseconds;

            GenGraType genGra = DeserializeGenGraXML();

            long timeAfterXmlDeserialization = stopwatch.ElapsedMilliseconds;
            long xmlDeserializationTime = timeAfterXmlDeserialization - timeBeforeXmlDeserialization;
            Debug.Log($"XML deserialization completed in: {xmlDeserializationTime}ms");

            long timeBeforeGraphTransformation = stopwatch.ElapsedMilliseconds;

            GraphType missionGraph = genGra.GenerateGraph();

            long timeAfterGraphTransformation = stopwatch.ElapsedMilliseconds;
            long graphTransformationTime = timeAfterGraphTransformation - timeBeforeGraphTransformation;
            DebugLogGraph(missionGraph);
            Debug.Log($"Mission graph generation completed in: {graphTransformationTime}ms");

            long timeBeforeSpaceGeneration = stopwatch.ElapsedMilliseconds;

            IDictionary<string, GameObject[]> generatedSpace = GenerateSpace(missionGraph);

            long timeAfterSpaceGeneration = stopwatch.ElapsedMilliseconds;
            long spaceGenerationTime = timeAfterSpaceGeneration - timeBeforeSpaceGeneration;
            Debug.Log($"Space generation completed in: {spaceGenerationTime}ms");

            long timeBeforePostProcessing = stopwatch.ElapsedMilliseconds;
            
            PostProcess(missionGraph, generatedSpace);

            long timeAfterPostProcessing = stopwatch.ElapsedMilliseconds;
            long postProcessingTime = timeAfterPostProcessing - timeBeforePostProcessing;
            Debug.Log($"Post processing completed in: {postProcessingTime}ms");

            stopwatch.Stop();
            Debug.Log($"Total execution completed in: {stopwatch.ElapsedMilliseconds}ms");
        }

        private GenGraType DeserializeGenGraXML()
        {
            using (FileStream fileStream = new FileStream(missionGraphGrammarFilePath, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GenGraType));
                return (GenGraType) serializer.Deserialize(fileStream);
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
        private IDictionary<string, GameObject[]> GenerateSpace(GraphType missionGraph)
        {
            int numStartNodes = missionGraph.StartNodes.Length;
            if (numStartNodes > 1)
            {
                throw new InvalidOperationException("Mission graph cannot have more than 1 start node. " +
                                                    $"It currently has {numStartNodes}.");
            }
            return BreadthFirstSpaceGeneration(missionGraph, missionGraph.StartNodes[0]);
        }

        private IDictionary<string, GameObject[]> BreadthFirstSpaceGeneration(GraphType graph, NodeType startNode)
        {
            IDictionary<string, GameObject[]> generatedSpace = new Dictionary<string, GameObject[]>(graph.Nodes.Node.Length);
            Queue<string> nodeIdQueue = new Queue<string>();

            GameObject[] startNodeSpaceObjects = BuildSpaceForMissionSymbol(startNode.symbol);
            generatedSpace[startNode.id] = startNodeSpaceObjects;
            nodeIdQueue.Enqueue(startNode.id);

            while (nodeIdQueue.Count != 0)
            {
                string currentNodeId = nodeIdQueue.Dequeue();
                IList<NodeType> adjacentNodes = graph.AdjacencyList[currentNodeId];
                foreach (NodeType adjacentNode in adjacentNodes)
                {
                    if (generatedSpace.ContainsKey(adjacentNode.id)) continue;
                    GameObject[] currentNodeSpaceObjects = generatedSpace[currentNodeId];
                    GameObject[] adjacentNodeSpaceObjects = BuildSpaceForMissionSymbol(
                        adjacentNode.symbol, currentNodeSpaceObjects);
                    generatedSpace[adjacentNode.id] = adjacentNodeSpaceObjects;
                    nodeIdQueue.Enqueue(adjacentNode.id);
                }
            }

            return generatedSpace;
        }

        private GameObject[] BuildSpaceForMissionSymbol(string missionSymbol, GameObject[] relativeSpaceObjects = null)
        {
            BuildingInstructions buildingInstructions = GetRandomBuildingInstructions(missionSymbol);
            return buildingInstructions.Build(relativeSpaceObjects);
        }
        
        private BuildingInstructions GetRandomBuildingInstructions(string missionSymbol)
        {
            BuildingInstructions[] buildingInstructionsArray = GetBuildingInstructions(missionSymbol);

            if (buildingInstructionsArray.Length == 1)
            {
                return buildingInstructionsArray[0];
            }
            
            int totalWeighting = 0;
            foreach (BuildingInstructions buildingInstructions in buildingInstructionsArray)
            {
                if (buildingInstructions.Weighting < 1)
                {
                    buildingInstructions.Weighting = 1;
                }

                totalWeighting += buildingInstructions.Weighting;
            }
            
            int desiredWeighting = Random.Range(1, totalWeighting + 1);
            int currentWeighting = 0;
            
            foreach (BuildingInstructions buildingInstructions in buildingInstructionsArray)
            {
                currentWeighting += buildingInstructions.Weighting;
                if (desiredWeighting <= currentWeighting)
                {
                    return buildingInstructions;
                }
            }

            throw new InvalidOperationException("Could not determine building instructions " +
                                                $"for weighting {desiredWeighting}");
        }

        private BuildingInstructions[] GetBuildingInstructions(string missionSymbol)
        {
            GameObject biPrefab;
            try
            {
                biPrefab = BuildingInstructionsByMissionSymbol[missionSymbol];
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException("No building instructions prefab found for mission symbol " +
                                                    $"'{missionSymbol}'. Please check validity of Building " +
                                                    "Instructions By Mission Symbol array.");
            }
            
            BuildingInstructions[] buildingInstructionsArray = biPrefab.GetComponents<BuildingInstructions>();
            if (buildingInstructionsArray == null || buildingInstructionsArray.Length == 0)
            {
                throw new InvalidOperationException("No BuildingInstructions component found attached to " +
                                                    $"{biPrefab}. Please check validity of this prefab.");
            }

            return buildingInstructionsArray;
        }

        private void PostProcess(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            foreach (PostProcessor postProcessor in GetComponents<PostProcessor>())
            {
                postProcessor.Process(missionGraph, generatedSpace);
            }
        }

        /**
         * This struct is a workaround to allow GameObjects to be mapped to a string in the Unity editor in a
         * similar vein to using a Dictionary, because IDictionary is not serializable by the Unity engine.
         */
        [Serializable]
        private class BuildingInstructionsHolder
        {
            [SerializeField] private string missionSymbol;
            [SerializeField] private GameObject buildingInstructionsPrefab;

            public string MissionSymbol => missionSymbol;

            public GameObject BuildingInstructionsPrefab => buildingInstructionsPrefab;
        }
    }
}