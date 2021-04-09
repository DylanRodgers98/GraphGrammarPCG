using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SingleBuildingInstructions : BuildingInstructions
{
    public override GameObject[] Build(GameObject[] relativeSpaceObjects = null)
    {
        ValidateSpaceObjectPrefab();
        Debug.Log($"[Single Building Instructions] Space Object: {spaceObjectPrefab}");

        // if no relative space objects exist, instantiate this space object at (0, 0, 0)
        // as this should be the first instantiated space object (TODO: add validation around this assumption)
        if (relativeSpaceObjects == null || relativeSpaceObjects.Length == 0)
        {
            return new []{Instantiate(spaceObjectPrefab, Vector3.zero, Quaternion.identity)};
        }
        
        
        /*
         * Choose random attachment point on relative object and the space object to be instantiated
         */
        
        Transform[] thisAttachmentPoints = spaceObjectPrefab.transform.Cast<Transform>()
            .Where(child => child.CompareTag("AttachmentPoint"))
            .ToArray();

        if (thisAttachmentPoints.Length == 0)
        {
            throw new InvalidOperationException("Space Object Prefab has no child objects with the the " +
                                                "'AttachmentPoint' tag. This tag is required to instantiated the " +
                                                "prefab attached to existing GameObjects in the scene.");
        }
        
        Transform[] relativeAttachmentPoints = relativeSpaceObjects
            .SelectMany(relativeSpaceObject => relativeSpaceObject.transform.Cast<Transform>())
            .Where(child => child.CompareTag("AttachmentPoint"))
            .ToArray();

        if (relativeAttachmentPoints.Length == 0)
        {
            throw new InvalidOperationException("No relative space objects have a child object with the the " +
                                                "'AttachmentPoint' tag. This tag is required to instantiated the " +
                                                "prefab attached to existing GameObjects in the scene.");
        }

        Transform thisAttachmentPoint = thisAttachmentPoints[Random.Range(0, thisAttachmentPoints.Length)];
        Transform relativeAttachmentPoint = relativeAttachmentPoints[Random.Range(0, relativeAttachmentPoints.Length)];

        
        /*
         * Calculate position and rotation of space object to be instantiated
         */
        
        Vector3 thisObjectPosition = spaceObjectPrefab.transform.position;
        Vector3 thisAttachmentPointVectorFromCenter = thisAttachmentPoint.position - thisObjectPosition;

        Vector3 relativeObjectPosition = relativeAttachmentPoint.parent.position;
        Vector3 relativeAttachmentPointVectorFromCenter = relativeAttachmentPoint.position - relativeObjectPosition;

        Vector3 vectorBetweenCenters = thisAttachmentPointVectorFromCenter + relativeAttachmentPointVectorFromCenter;

        Vector3 spaceObjectPosition = relativeObjectPosition + vectorBetweenCenters;
        Quaternion spaceObjectRotation = Quaternion.Euler(0, relativeAttachmentPoint.eulerAngles.y - 180, 0);
        
        return new []{Instantiate(spaceObjectPrefab, spaceObjectPosition, spaceObjectRotation)};
    }
}
