using GenGra;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class GenGraParser : MonoBehaviour
{
    public string _graphFilePath;

    // Start is called before the first frame update
    void Start()
    {
        GenGraType genGra;
        using (FileStream fileStream = new FileStream(_graphFilePath, FileMode.Open))
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            XmlSerializer serializer = new XmlSerializer(typeof(GenGraType));
            genGra = (GenGraType)serializer.Deserialize(fileStream);

            stopwatch.Stop();
            Debug.Log($"XML deserialization completed in: {stopwatch.ElapsedMilliseconds}ms");
        }
        DebugLogDeserialization(genGra);
    }

    // TODO: remove this method when done prototyping
    private void DebugLogDeserialization(GenGraType genGra)
    {
        foreach (GraphType graph in genGra.Graphs.Graph)
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
        Debug.Log($"[Grammar] StartGraph: {genGra.Grammar.StartGraph.@ref}");
        RuleType[] rules = genGra.Grammar.Rules.Rule;
        for (int i = 0; i < rules.Length; i++)
        {
            RuleType rule = rules[i];
            Debug.Log($"[Grammar | Rule {i + 1}] source: {rule.source} | target: {rule.target}");
        }
    }

}
