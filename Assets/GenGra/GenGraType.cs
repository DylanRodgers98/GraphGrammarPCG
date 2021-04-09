using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenGra
{
    public partial class GenGraType
    {
        public GraphType GenerateGraph()
        {
            IDictionary<string, GraphType> graphs = new Dictionary<string, GraphType>(Graphs.Graph.Length);
            foreach (GraphType graph in Graphs.Graph)
            {
                graphs[graph.id] = graph;
            }

            string startGraphRef = Grammar.StartGraph.@ref;
            GraphType startGraph = graphs[startGraphRef];

            int ruleNumber = 0;

            while (true)
            {
                RuleType[] applicableRules = GetApplicableRules(graphs, startGraph);
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

        private RuleType[] GetApplicableRules(IDictionary<string, GraphType> graphs, GraphType startGraph)
        {
            return Grammar.Rules.Rule
                .Where(rule =>
                {
                    GraphType ruleSourceGraph = graphs[rule.source];
                    return startGraph.IsSupergraphOf(ruleSourceGraph);
                })
                .ToArray();
        }
    }
}