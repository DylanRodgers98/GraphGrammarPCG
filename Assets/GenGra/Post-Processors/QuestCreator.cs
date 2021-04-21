using System.Collections.Generic;
using UnityEngine;

namespace GenGra
{
    public class QuestCreator : PostProcessor
    {
        [SerializeField] private string questItemSymbol;
        [SerializeField] private string levelBossSymbol;
        private PlayerController playerController;

        public override void Process(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            
            if (missionGraph.NodeSymbolMap.ContainsKey(questItemSymbol))
            {
                CreateQuestsForQuestItems(missionGraph, generatedSpace);
            }

            if (missionGraph.NodeSymbolMap.ContainsKey(levelBossSymbol))
            {
                CreateQuestsForLevelBoss(missionGraph, generatedSpace);
            }
        }

        private void CreateQuestsForQuestItems(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
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

        private void CreateQuestsForLevelBoss(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            IList<NodeType> levelBossNodes = missionGraph.NodeSymbolMap[levelBossSymbol];
            foreach (NodeType levelBossNode in levelBossNodes)
            {
                GameObject[] spaceObjects = generatedSpace[levelBossNode.id];
                foreach (GameObject spaceObject in spaceObjects)
                {
                    foreach (Transform child in spaceObject.transform)
                    {
                        if (child.CompareTag("Enemy"))
                        {
                            Enemy enemy = child.GetComponent<Enemy>();
                            Quest quest = new Quest($"Defeat {enemy.EnemyName}");
                            enemy.Quest = quest;
                            playerController.AddQuest(quest);
                        }
                    }
                }
            }
        }
    }
}