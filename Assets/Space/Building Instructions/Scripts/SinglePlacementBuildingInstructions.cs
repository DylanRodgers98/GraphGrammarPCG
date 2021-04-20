using System.Collections.Generic;
using UnityEngine;

public class SinglePlacementBuildingInstructions : BuildingInstructions
{
    public override GameObject[] Build(GameObject[] relativeSpaceObjects = null, bool checkForOverlap = true)
    {
        ValidateSpaceObjectPrefab();

        GameObject spaceObjectPrefab = GetRandomSpaceObject();

        // if no relative space objects exist, instantiate this space object at (0, 0, 0)
        // as this should be the first instantiated space object (TODO: add validation around this assumption)
        if (relativeSpaceObjects == null || relativeSpaceObjects.Length == 0)
        {
            return new[] {Instantiate(spaceObjectPrefab, Vector3.zero, Quaternion.identity)};
        }

        Transform entrancePoint = GetRandomEntrancePoint(spaceObjectPrefab);
        IList<Transform> relativeExitPoints = GetExitPoints(relativeSpaceObjects);

        IList<Transform> availableExitPoints;
        bool isTryingNonRelativeExitPoints;

        if (relativeExitPoints.Count == 0)
        {
            availableExitPoints = GetNonRelativeExitPoints();
            isTryingNonRelativeExitPoints = true;
        }
        else
        {
            availableExitPoints = new List<Transform>(relativeExitPoints);
            isTryingNonRelativeExitPoints = false;
        }

        while (availableExitPoints.Count > 0)
        {
            Transform exitPoint = GetRandomAttachmentPoint(availableExitPoints);
            Quaternion spaceObjectRotation = CalculateInstantiationRotation(entrancePoint, exitPoint);
            GameObject instantiated = InstantiateSpaceObject(
                exitPoint.position, spaceObjectRotation, entrancePoint, checkForOverlap);

            if (instantiated != null)
            {
                DestroyImmediate(exitPoint.gameObject);
                return new[] {instantiated};
            }

            availableExitPoints.Remove(exitPoint);
            if (availableExitPoints.Count > 0 || isTryingNonRelativeExitPoints) continue;

            availableExitPoints = GetNonRelativeExitPoints(relativeExitPoints);
            isTryingNonRelativeExitPoints = true;
        }

        throw new CannotBuildException("There are no available exit points to attach the GameObject to, " +
                                       $"so cannot instantiate {spaceObjectPrefab}");
    }

    private static IList<Transform> GetNonRelativeExitPoints(ICollection<Transform> relativeAttachmentPoints = null)
    {
        IList<Transform> nonRelativeAttachmentPoints = new List<Transform>();

        GameObject[] attachmentPoints = GameObject.FindGameObjectsWithTag(AttachmentPointTag);
        GameObject[] exitPoints = GameObject.FindGameObjectsWithTag(ExitPointTag);

        foreach (GameObject attachmentPoint in attachmentPoints)
        {
            if (relativeAttachmentPoints == null || !relativeAttachmentPoints.Contains(attachmentPoint.transform))
            {
                nonRelativeAttachmentPoints.Add(attachmentPoint.transform);
            }
        }

        foreach (GameObject exitPoint in exitPoints)
        {
            if (relativeAttachmentPoints == null || !relativeAttachmentPoints.Contains(exitPoint.transform))
            {
                nonRelativeAttachmentPoints.Add(exitPoint.transform);
            }
        }

        return nonRelativeAttachmentPoints;
    }
}