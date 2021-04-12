using System.Collections.Generic;
using UnityEngine;

public class SinglePlacementBuildingInstructions : BuildingInstructions
{
    public override GameObject[] Build(GameObject[] relativeSpaceObjects = null)
    {
        ValidateSpaceObjectPrefab();
        Debug.Log($"[Single Placement Building Instructions] Space Object: {spaceObjectPrefab}");

        // if no relative space objects exist, instantiate this space object at (0, 0, 0)
        // as this should be the first instantiated space object (TODO: add validation around this assumption)
        if (relativeSpaceObjects == null || relativeSpaceObjects.Length == 0)
        {
            return new[] {Instantiate(spaceObjectPrefab, Vector3.zero, Quaternion.identity)};
        }

        Transform thisAttachmentPoint = GetRandomEntrancePoint(spaceObjectPrefab);
        IList<Transform> relativeAttachmentPoints = GetExitPoints(relativeSpaceObjects);

        IList<Transform> otherAttachmentPoints = new List<Transform>(relativeAttachmentPoints);
        bool isTryingNonRelativeAttachmentPoints = false;

        while (otherAttachmentPoints.Count > 0)
        {
            Transform otherAttachmentPoint = GetRandomAttachmentPoint(otherAttachmentPoints);
            Quaternion spaceObjectRotation = CalculateInstantiationRotation(thisAttachmentPoint, otherAttachmentPoint);
            GameObject instantiated = InstantiateSpaceObject(otherAttachmentPoint.position, spaceObjectRotation, thisAttachmentPoint);

            if (instantiated != null)
            {
                DestroyImmediate(otherAttachmentPoint.gameObject);
                return new[] {instantiated};
            }

            otherAttachmentPoints.Remove(otherAttachmentPoint);
            if (otherAttachmentPoints.Count > 0 || isTryingNonRelativeAttachmentPoints) continue;
            
            Debug.LogWarning("Exhausted all relative attachment points. Trying non-relative attachment points.");
            otherAttachmentPoints = GetNonRelativeAttachmentPoints(relativeAttachmentPoints);
            isTryingNonRelativeAttachmentPoints = true;
        }

        throw new CannotBuildException("There are no available attachment points to attach the GameObject to, " +
                                       $"so cannot instantiate {spaceObjectPrefab}.");
    }

    private static IList<Transform> GetNonRelativeAttachmentPoints(ICollection<Transform> relativeAttachmentPoints)
    {
        IList<Transform> nonRelativeAttachmentPoints = new List<Transform>();
        
        GameObject[] attachmentPoints = GameObject.FindGameObjectsWithTag(AttachmentPointTag);
        GameObject[] exitPoints = GameObject.FindGameObjectsWithTag(ExitPointTag);
                
        foreach (GameObject attachmentPoint in attachmentPoints)
        {
            if (!relativeAttachmentPoints.Contains(attachmentPoint.transform))
            {
                nonRelativeAttachmentPoints.Add(attachmentPoint.transform);
            }
        }

        foreach (GameObject exitPoint in exitPoints)
        {
            if (!relativeAttachmentPoints.Contains(exitPoint.transform))
            {
                nonRelativeAttachmentPoints.Add(exitPoint.transform);
            }
        }

        return nonRelativeAttachmentPoints;
    }
}