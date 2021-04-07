using GenGra;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenGraParser : MonoBehaviour
{
    public string graphFilePath;
    
    // Start is called before the first frame update
    void Start()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        GenGraType genGra;
        using (FileStream fileStream = new FileStream(graphFilePath, FileMode.Open))
        {
            stopwatch.Start();

            XmlSerializer serializer = new XmlSerializer(typeof(GenGraType));
            genGra = (GenGraType)serializer.Deserialize(fileStream);

            Debug.Log($"XML deserialization completed in: {stopwatch.ElapsedMilliseconds}ms");
        }
        DebugLogDeserialization(genGra);

        IDictionary<string, GraphType> graphs = new Dictionary<string, GraphType>();
        foreach (GraphType graph in genGra.Graphs.Graph)
        {
            graphs[graph.id] = graph;
        }

        string startGraphRef = genGra.Grammar.StartGraph.@ref;
        GraphType startGraph = graphs[startGraphRef];

        RuleType[] applicableRules = GetApplicableRules(genGra, graphs, startGraph);
        
        foreach (RuleType applicableRule in applicableRules)
        {
            Debug.Log($"[Applicable Rule] source: {applicableRule.source} | target: {applicableRule.target}");
        }

        while (applicableRules.Length > 0)
        {
            RuleType ruleToApply = applicableRules.Length == 1
                ? applicableRules[0]
                : applicableRules[Random.Range(0, applicableRules.Length - 1)];

            GraphType ruleSource = graphs[ruleToApply.source];
            GraphType ruleTarget = graphs[ruleToApply.target];
            startGraph.FindAndReplace(ruleSource, ruleTarget);
            
            applicableRules = GetApplicableRules(genGra, graphs, startGraph);
        }

        DebugLogGraph(startGraph);
        
        stopwatch.Stop();
        Debug.Log($"Total execution completed in: {stopwatch.ElapsedMilliseconds}ms");
    }

    private RuleType[] GetApplicableRules(GenGraType genGra, IDictionary<string, GraphType> graphs, GraphType startGraph)
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

    private void DebugLogGraph(GraphType graph)
    {
        foreach (NodeType node in graph.Nodes.Node)
        {
            Debug.Log($"[Graph {graph.id} | Node: {node.id}] symbol: {node.symbol}");
        }
        EdgeType[] edges = graph.Edges.Edge;
        for (int j = 0; j < edges.Length; j++)
        {
            EdgeType edge = edges[j];
            Debug.Log($"[Graph {graph.id} | Edge: {j + 1}] source: {edge.source} | target: {edge.target}");
        }
    }

}
