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

        Transform thisAttachmentPoint = GetRandomAttachmentPoint(spaceObjectPrefab);
        IList<Transform> relativeAttachmentPoints = GetAttachmentPoints(relativeSpaceObjects);

        while (relativeAttachmentPoints.Count > 0)
        {
            Transform relativeAttachmentPoint = GetRandomAttachmentPoint(relativeAttachmentPoints);
            Quaternion spaceObjectRotation = CalculateInstantiationRotation(thisAttachmentPoint, relativeAttachmentPoint);
            GameObject instantiated = Instantiate(spaceObjectPrefab, relativeAttachmentPoint.position, spaceObjectRotation);
            Transform instantiatedAttachmentPoint = instantiated.transform.Find(thisAttachmentPoint.name);
            Vector3 translation = instantiated.transform.position - instantiatedAttachmentPoint.position;
            instantiated.transform.Translate(translation, Space.World);

            if (Physics.OverlapBox(instantiated.transform.position, instantiated.transform.localScale / 2 * 0.99f).Length > 1)
            {
                DestroyImmediate(instantiated);
                relativeAttachmentPoints.Remove(relativeAttachmentPoint);
            }
            else
            {
                DestroyAttachmentPoints(instantiatedAttachmentPoint, relativeAttachmentPoint);
                return new[] {instantiated};
            }
        }
        
        throw new CannotBuildException("There are no available attachment points to attach the GameObject to. " +
                                       "This may be because the scene has no space near the relative space objects " +
                                       $"to instantiate {spaceObjectPrefab}.");
    }
}
