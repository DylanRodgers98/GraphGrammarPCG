using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenGra
{
    public class DungeonCleanup : PostProcessor
    {
        [SerializeField] private GameObject endWallBuildingInstructionsPrefab;
        private BuildingInstructions endWallBuildingInstructions;

        public override void Process(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            DestroyAttachedAttachmentPoints();
            AttachWallsToUnattachedAttachmentPoints(generatedSpace);
        }

        private static void DestroyAttachedAttachmentPoints()
        {
            IEnumerable<GameObject> attachmentPointsToDestroy = GetAttachmentPointsByPosition()
                .Select(entry => entry.Value)
                .Where(list => list.Count > 1)
                .SelectMany(list => list);

            foreach (GameObject attachmentPoint in attachmentPointsToDestroy)
            {
                DestroyImmediate(attachmentPoint);
            }
        }

        private static IDictionary<Vector3, IList<GameObject>> GetAttachmentPointsByPosition()
        {
            IDictionary<Vector3, IList<GameObject>> attachmentPointsByPosition =
                new Dictionary<Vector3, IList<GameObject>>();
            GetAttachmentPointsByPosition(attachmentPointsByPosition, BuildingInstructions.AttachmentPointTag);
            GetAttachmentPointsByPosition(attachmentPointsByPosition, BuildingInstructions.EntrancePointTag);
            GetAttachmentPointsByPosition(attachmentPointsByPosition, BuildingInstructions.ExitPointTag);
            return attachmentPointsByPosition;
        }

        private static void GetAttachmentPointsByPosition(IDictionary<Vector3, 
                IList<GameObject>> attachmentPointsByPosition, string attachmentPointTag)
        {
            foreach (GameObject attachmentPoint in GameObject.FindGameObjectsWithTag(attachmentPointTag))
            {
                Vector3 position = attachmentPoint.transform.position;
                if (!attachmentPointsByPosition.ContainsKey(position))
                {
                    attachmentPointsByPosition[position] = new List<GameObject>();
                }

                attachmentPointsByPosition[position].Add(attachmentPoint);
            }
        }

        private void AttachWallsToUnattachedAttachmentPoints(IDictionary<string, GameObject[]> generatedSpace)
        {
            foreach (KeyValuePair<string, GameObject[]> kvp in generatedSpace)
            {
                GameObject[] spaceObjects = kvp.Value;
                foreach (GameObject spaceObject in spaceObjects)
                {
                    AttachWallsTo(spaceObject);
                }
            }
        }
        
        private void AttachWallsTo(GameObject spaceObjectToAttachWallTo)
        {
            if (endWallBuildingInstructions == null)
            {
                endWallBuildingInstructions = endWallBuildingInstructionsPrefab
                    .GetComponent<SinglePlacementBuildingInstructions>();
                
                if (endWallBuildingInstructions == null)
                {
                    throw new InvalidOperationException("No BuildingInstructions component found attached to " +
                                                        $"{endWallBuildingInstructionsPrefab}. Please check validity " +
                                                        "of this prefab.");
                }
            }
            
            int numExitPoints = spaceObjectToAttachWallTo.transform.Cast<Transform>()
                .Count(child => child.CompareTag(BuildingInstructions.AttachmentPointTag) || 
                                child.CompareTag(BuildingInstructions.ExitPointTag));
            
            for (int i = 0; i < numExitPoints; i++)
            {
                GameObject[] built = endWallBuildingInstructions.Build(new[] {spaceObjectToAttachWallTo}, false);
                foreach (GameObject obj in built)
                {
                    obj.transform.parent = spaceObjectToAttachWallTo.transform;
                }
            }
        }
    }
}