using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GenGra
{
    public class GenGraParser : MonoBehaviour
    {
        [SerializeField]
        private string graphFilePath;

        void Start()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            
            GenGraType genGra;
            using (FileStream fileStream = new FileStream(graphFilePath, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GenGraType));
                genGra = (GenGraType) serializer.Deserialize(fileStream);

                Debug.Log($"XML deserialization completed in: {stopwatch.ElapsedMilliseconds}ms");
            }

            DebugLogDeserialization(genGra);

            GraphType finalGraph = TransformGraph(genGra);

            DebugLogGraph(finalGraph);

            stopwatch.Stop();
            Debug.Log($"Total execution completed in: {stopwatch.ElapsedMilliseconds}ms");
        }

        // TODO: move this method to be an instance method on GenGraType
        private GraphType TransformGraph(GenGraType genGra)
        {
            IDictionary<string, GraphType> graphs = new Dictionary<string, GraphType>();
            foreach (GraphType graph in genGra.Graphs.Graph)
            {
                graphs[graph.id] = graph;
            }

            string startGraphRef = genGra.Grammar.StartGraph.@ref;
            GraphType startGraph = graphs[startGraphRef];

            int ruleNumber = 0;

            while (true)
            {
                RuleType[] applicableRules = GetApplicableRules(genGra, graphs, startGraph);
                if (applicableRules.Length == 0) return startGraph;

                RuleType ruleToApply = applicableRules.Length == 1
                    ? applicableRules[0]
                    : applicableRules[Random.Range(0, applicableRules.Length - 1)];

                Debug.Log($"[Applying Rule {++ruleNumber}] source: {ruleToApply.source} | target: {ruleToApply.target}");

                GraphType ruleSource = graphs[ruleToApply.source];
                GraphType ruleTarget = graphs[ruleToApply.target];
                startGraph.FindAndReplace(ruleSource, ruleTarget);
            }
        }

        // TODO: move this method to be an instance method on GenGraType
        private static RuleType[] GetApplicableRules(GenGraType genGra, IDictionary<string, GraphType> graphs,
            GraphType startGraph)
        {
            return genGra.Grammar.Rules.Rule
                .Where(rule =>
                {
                    GraphType ruleSourceGraph = graphs[rule.source];
                    return startGraph.IsSupergraphOf(ruleSourceGraph);
                })
                .ToArray();
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

            EdgeType[] edges = graph.Edges?.Edge ?? new EdgeType[0];
            for (int j = 0; j < edges.Length; j++)
            {
                EdgeType edge = edges[j];
                Debug.Log($"[Graph {graph.id} | Edge: {j + 1}] source: {edge.source} | target: {edge.target}");
            }
        }
    }
}