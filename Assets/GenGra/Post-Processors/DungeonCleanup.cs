using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenGra
{
    public class DungeonCleanup : PostProcessor
    {
        private const string LockTag = "Lock";
        private const string KeyTag = "Key";
        private const string ItemTag = "Item";
        private const string EnemyTag = "Enemy";
        private const string TestTag = "Test";

        [SerializeField] private GameObject endWallPrefab;
        [SerializeField] private GameObject corridorPrefab;
        [SerializeField] private GameObject corridorEndPrefab;

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
            IList<GameObject> exitPoints = new List<GameObject>();
            bool isEmptyCrossroads = true;

            foreach (Transform child in crossroadsSpaceObject.transform)
            {
                switch (child.tag)
                {
                    case BuildingInstructions.AttachmentPointTag:
                    case BuildingInstructions.ExitPointTag:
                        exitPoints.Add(child.gameObject);
                        break;
                    case LockTag:
                    case KeyTag:
                    case ItemTag:
                    case TestTag:
                    case EnemyTag:
                        isEmptyCrossroads = false;
                        break;
                }
            }

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
                    GameObject instantiated = ReplaceSpaceObject(crossroadsSpaceObject, corridorPrefab);
                    IList<GameObject> instantiatedAttachmentPoints = instantiated.transform.Cast<Transform>()
                        .Where(child => child.CompareTag(BuildingInstructions.AttachmentPointTag) ||
                                        child.CompareTag(BuildingInstructions.ExitPointTag))
                        .Select(child => child.gameObject)
                        .ToList();

                    if (instantiatedAttachmentPoints.Count != 2)
                    {
                        throw new InvalidOperationException($"Instantiated corridor {instantiated} has " +
                                                            $"{instantiatedAttachmentPoints.Count} attachment " +
                                                            "points or exit points but was expecting 2");
                    }

                    if (aX.Equals(bX))
                    {
                        float aXInstantiated = instantiatedAttachmentPoints[0].transform.position.x;
                        float bXInstantiated = instantiatedAttachmentPoints[1].transform.position.x;
                        RotateInstantiated(instantiated, aXInstantiated, aX, bXInstantiated, bX);
                    }
                    else if (aZ.Equals(bZ))
                    {
                        float aZInstantiated = instantiatedAttachmentPoints[0].transform.position.z;
                        float bZInstantiated = instantiatedAttachmentPoints[1].transform.position.z;
                        RotateInstantiated(instantiated, aZInstantiated, aZ, bZInstantiated, bZ);
                    }

                    AttachWallsToOriginalIfOverlap(instantiated, crossroadsSpaceObject,
                        exitPoints.Count, nodeId, generatedSpace);
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
            IList<GameObject> exitPoints = new List<GameObject>();
            bool isEmptyTJunction = true;

            foreach (Transform child in tJunctionSpaceObject.transform)
            {
                switch (child.tag)
                {
                    case BuildingInstructions.AttachmentPointTag:
                    case BuildingInstructions.ExitPointTag:
                        exitPoints.Add(child.gameObject);
                        break;
                    case LockTag:
                    case KeyTag:
                    case ItemTag:
                    case TestTag:
                    case EnemyTag:
                        isEmptyTJunction = false;
                        break;
                }

                if (exitPoints.Count == 2 && !isEmptyTJunction) break;
            }

            if (isEmptyTJunction && exitPoints.Count == 2)
            {
                GameObject instantiated = ReplaceSpaceObject(tJunctionSpaceObject, corridorEndPrefab);
                AttachWallsToOriginalIfOverlap(instantiated, tJunctionSpaceObject,
                    exitPoints.Count, nodeId, generatedSpace);
            }
            else
            {
                AttachWallTo(tJunctionSpaceObject, exitPoints.Count);
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
            BuildingInstructions wallBuildingInstructions = GetComponent<SinglePlacementBuildingInstructions>();
            if (wallBuildingInstructions == null)
            {
                wallBuildingInstructions = gameObject.AddComponent<SinglePlacementBuildingInstructions>();
                wallBuildingInstructions.SpaceObjectVariants = new[]
                {
                    new BuildingInstructions.WeightedSpaceObject {SpaceObject = endWallPrefab}
                };
            }

            GameObject[] built = wallBuildingInstructions.Build(new[] {spaceObjectToAttachWallTo}, false);
            foreach (GameObject obj in built)
            {
                obj.transform.parent = spaceObjectToAttachWallTo.transform;
            }
        }

        private static GameObject ReplaceSpaceObject(GameObject spaceObject, GameObject prefabToInstantiate)
        {
            Vector3 position = spaceObject.transform.position;
            Quaternion rotation = spaceObject.transform.rotation;
            spaceObject.SetActive(false);
            return Instantiate(prefabToInstantiate, position, rotation);
        }

        private void AttachWallsToOriginalIfOverlap(GameObject gameObjectToCheck, GameObject originalSpaceObject,
            int numExitPoints, string nodeId, IDictionary<string, GameObject[]> generatedSpace)
        {
            if (BuildingInstructions.DoesInstantiatedOverlapOtherSpaceObjects(gameObjectToCheck))
            {
                DestroyImmediate(gameObjectToCheck);
                originalSpaceObject.SetActive(true);
                AttachWallTo(originalSpaceObject, numExitPoints);
            }
            else
            {
                List<GameObject> spaceObjects = generatedSpace[nodeId].ToList();
                spaceObjects.Remove(originalSpaceObject);
                spaceObjects.Add(gameObjectToCheck);
                generatedSpace[nodeId] = spaceObjects.ToArray();
                DestroyImmediate(originalSpaceObject);
            }
        }

        private void RotateInstantiated(GameObject instantiated, float a1, float b1, float a2, float b2)
        {
            while (!a1.Equals(b1) && !a2.Equals(b2))
            {
                float instantiatedYRotation = instantiated.transform.eulerAngles.y;
                instantiated.transform.rotation = Quaternion.Euler(0, instantiatedYRotation + 90, 0);
            }
        }
    }
}