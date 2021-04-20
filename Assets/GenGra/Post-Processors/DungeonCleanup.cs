using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenGra
{
    public class DungeonCleanup : PostProcessor
    {
        [SerializeField] private GameObject endWallBuildingInstructions;
        [SerializeField] private GameObject corridorBuildingInstructions;
        [SerializeField] private GameObject corridorEndBuildingInstructions;

        public override void Process(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            DestroyAttachedAttachmentPoints();
            CleanUpLooseExits(generatedSpace);
        }

        private void DestroyAttachedAttachmentPoints()
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

        private IDictionary<Vector3, IList<GameObject>> GetAttachmentPointsByPosition()
        {
            IDictionary<Vector3, IList<GameObject>> attachmentPointsByPosition =
                new Dictionary<Vector3, IList<GameObject>>();
            GetAttachmentPointsByPosition(attachmentPointsByPosition, BuildingInstructions.AttachmentPointTag);
            GetAttachmentPointsByPosition(attachmentPointsByPosition, BuildingInstructions.EntrancePointTag);
            GetAttachmentPointsByPosition(attachmentPointsByPosition, BuildingInstructions.ExitPointTag);
            return attachmentPointsByPosition;
        }

        private void GetAttachmentPointsByPosition(IDictionary<Vector3, IList<GameObject>> attachmentPointsByPosition,
            string attachmentPointTag)
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

        private void CleanUpLooseExits(IDictionary<string, GameObject[]> generatedSpace)
        {
            IDictionary<string, GameObject[]> newGeneratedSpace = new Dictionary<string, GameObject[]>(generatedSpace);

            foreach (KeyValuePair<string, GameObject[]> kvp in generatedSpace)
            {
                string nodeId = kvp.Key;
                GameObject[] spaceObjects = kvp.Value;
                foreach (GameObject spaceObject in spaceObjects)
                {
                    switch (spaceObject.tag)
                    {
                        case "Corridor":
                            CleanUpCorridor(spaceObject);
                            break;
                        case "Crossroads":
                            CleanUpCrossroads(spaceObject, nodeId, newGeneratedSpace);
                            break;
                        case "T-Junction":
                            CleanUpTJunction(spaceObject, nodeId, newGeneratedSpace);
                            break;
                    }
                }
            }

            foreach (KeyValuePair<string, GameObject[]> kvp in newGeneratedSpace)
            {
                generatedSpace[kvp.Key] = kvp.Value;
            }
        }

        private void CleanUpCorridor(GameObject corridorSpaceObject)
        {
            bool hasUnattachedExitPoint = corridorSpaceObject.transform.Cast<Transform>()
                .Any(child => child.CompareTag(BuildingInstructions.AttachmentPointTag) ||
                              child.CompareTag(BuildingInstructions.ExitPointTag));

            if (hasUnattachedExitPoint)
            {
                AttachWallTo(corridorSpaceObject);
            }
        }

        private void CleanUpCrossroads(GameObject crossroadsSpaceObject, string nodeId,
            IDictionary<string, GameObject[]> generatedSpace)
        {
            (IList<GameObject> exitPoints, bool isEmptyCrossroads) = GetExitPointsAndEmptiness(crossroadsSpaceObject);

            if (exitPoints.Count == 0) return;

            if (isEmptyCrossroads && exitPoints.Count == 2)
            {
                float aX = exitPoints[0].transform.position.x;
                float aZ = exitPoints[0].transform.position.z;
                float bX = exitPoints[1].transform.position.x;
                float bZ = exitPoints[1].transform.position.z;

                if (!aX.Equals(bX) || !aZ.Equals(bZ))
                {
                    AttachWallTo(crossroadsSpaceObject, exitPoints.Count);
                }
                else
                {
                    GameObject replacement = ReplaceSpaceObject(
                        corridorBuildingInstructions, crossroadsSpaceObject, exitPoints.Count, nodeId, generatedSpace);

                    if (replacement != null)
                    {
                        IList<GameObject> instantiatedAttachmentPoints = replacement.transform.Cast<Transform>()
                            .Where(child => child.CompareTag(BuildingInstructions.AttachmentPointTag) ||
                                            child.CompareTag(BuildingInstructions.ExitPointTag))
                            .Select(child => child.gameObject)
                            .ToList();

                        if (instantiatedAttachmentPoints.Count != 2)
                        {
                            throw new InvalidOperationException($"Instantiated corridor {replacement} has " +
                                                                $"{instantiatedAttachmentPoints.Count} attachment " +
                                                                "points or exit points but was expecting 2");
                        }

                        if (aX.Equals(bX))
                        {
                            float aXInstantiated = instantiatedAttachmentPoints[0].transform.position.x;
                            float bXInstantiated = instantiatedAttachmentPoints[1].transform.position.x;
                            RotateInstantiated(replacement, aXInstantiated, aX, bXInstantiated, bX);
                        }
                        else if (aZ.Equals(bZ))
                        {
                            float aZInstantiated = instantiatedAttachmentPoints[0].transform.position.z;
                            float bZInstantiated = instantiatedAttachmentPoints[1].transform.position.z;
                            RotateInstantiated(replacement, aZInstantiated, aZ, bZInstantiated, bZ);
                        }
                    }
                }
            }
            else
            {
                AttachWallTo(crossroadsSpaceObject, exitPoints.Count);
            }
        }

        private void CleanUpTJunction(GameObject tJunctionSpaceObject, string nodeId,
            IDictionary<string, GameObject[]> generatedSpace)
        {
            (IList<GameObject> exitPoints, bool isEmptyTJunction) = GetExitPointsAndEmptiness(tJunctionSpaceObject);

            if (isEmptyTJunction && exitPoints.Count == 2)
            {
                ReplaceSpaceObject(
                    corridorEndBuildingInstructions, tJunctionSpaceObject, exitPoints.Count, nodeId, generatedSpace);
            }
            else
            {
                AttachWallTo(tJunctionSpaceObject, exitPoints.Count);
            }
        }

        private static Tuple<IList<GameObject>, bool> GetExitPointsAndEmptiness(GameObject spaceObject)
        {
            IList<GameObject> exitPoints = new List<GameObject>();
            bool isEmpty = true;

            foreach (Transform child in spaceObject.transform)
            {
                switch (child.tag)
                {
                    case BuildingInstructions.AttachmentPointTag:
                    case BuildingInstructions.ExitPointTag:
                        exitPoints.Add(child.gameObject);
                        break;
                    case "Lock":
                    case "Key":
                    case "Item":
                    case "Trap":
                    case "Enemy":
                        isEmpty = false;
                        break;
                }
            }

            return Tuple.Create(exitPoints, isEmpty);
        }

        private GameObject ReplaceSpaceObject(GameObject buildingInstructionsPrefab, GameObject spaceObjectToReplace,
            int numExitPoints, string nodeId, IDictionary<string, GameObject[]> generatedSpace)
        {
            BuildingInstructions buildingInstructions =
                buildingInstructionsPrefab.GetComponent<ReplacementBuildingInstructions>();

            if (buildingInstructions == null)
            {
                throw new InvalidOperationException("No BuildingInstructions component found attached " +
                                                    $"to {buildingInstructionsPrefab}. Please check validity of this " +
                                                    "prefab.");
            }

            GameObject[] built = buildingInstructions.Build(new[] {spaceObjectToReplace});
            if (built.Length == 0)
            {
                spaceObjectToReplace.SetActive(true);
                AttachWallTo(spaceObjectToReplace, numExitPoints);
                return null;
            }

            if (built.Length != 1)
            {
                throw new InvalidOperationException($"Expected BuildingInstructions {buildingInstructions} " +
                                                    $"to build 1 space object but it built {built.Length}");
            }

            GameObject replacement = built[0];

            List<GameObject> spaceObjects = generatedSpace[nodeId].ToList();
            spaceObjects.Remove(spaceObjectToReplace);
            spaceObjects.Add(replacement);
            generatedSpace[nodeId] = spaceObjects.ToArray();
            DestroyImmediate(spaceObjectToReplace);

            return replacement;
        }

        private void RotateInstantiated(GameObject instantiated, float a1, float b1, float a2, float b2)
        {
            while (!a1.Equals(b1) && !a2.Equals(b2))
            {
                float instantiatedYRotation = instantiated.transform.eulerAngles.y;
                instantiated.transform.rotation = Quaternion.Euler(0, instantiatedYRotation + 90, 0);
            }
        }

        private void AttachWallTo(GameObject spaceObjectToAttachWallTo, int multiplicity)
        {
            for (int i = 0; i < multiplicity; i++)
            {
                AttachWallTo(spaceObjectToAttachWallTo);
            }
        }

        private void AttachWallTo(GameObject spaceObjectToAttachWallTo)
        {
            BuildingInstructions wallBuildingInstructions =
                endWallBuildingInstructions.GetComponent<SinglePlacementBuildingInstructions>();

            if (wallBuildingInstructions == null)
            {
                throw new InvalidOperationException("No BuildingInstructions component found attached to " +
                                                    $"{endWallBuildingInstructions}. Please check validity of this " +
                                                    $"prefab.");
            }

            GameObject[] built = wallBuildingInstructions.Build(new[] {spaceObjectToAttachWallTo}, false);
            foreach (GameObject obj in built)
            {
                obj.transform.parent = spaceObjectToAttachWallTo.transform;
            }
        }
    }
}