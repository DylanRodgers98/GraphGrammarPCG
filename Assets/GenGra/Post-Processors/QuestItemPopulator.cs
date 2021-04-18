using System.Collections.Generic;
using UnityEngine;

namespace GenGra
{
    public class QuestItemPopulator : PostProcessor
    {
        [SerializeField] private string questItemSymbol;

        public override void Process(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            if (!missionGraph.NodeSymbolMap.ContainsKey(questItemSymbol)) return;

            PlayerController playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            
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
                            Item item = child.GetComponent<Item>();
                            Quest quest = new Quest($"Find {item.ItemName}");
                            item.Quest = quest;
                            playerController.AddQuest(quest);
                        }
                    }
                }
            }
        }
    }
}