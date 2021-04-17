using System.Collections.Generic;
using UnityEngine;

namespace GenGra
{
    public class QuestItemActivator : PostProcessor
    {
        [SerializeField] private string questItemSymbol;

        public override void Process(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            if (!missionGraph.NodeSymbolMap.ContainsKey(questItemSymbol)) return;
            
            IList<NodeType> questItemNodes = missionGraph.NodeSymbolMap[questItemSymbol];
            foreach (NodeType questItemNode in questItemNodes)
            {
                GameObject[] spaceObjects = generatedSpace[questItemNode.id];
                foreach (GameObject spaceObject in spaceObjects)
                {
                    foreach (Transform child in spaceObject.transform)
                    {
                        if (child.CompareTag("Item"))
                        {
                            child.GetComponent<Item>().IsQuestItem = true;
                        }
                    }
                }
            }
        }
    }
}