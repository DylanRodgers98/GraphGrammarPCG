using UnityEngine;

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
            return new[] {Instantiate(spaceObjectPrefab, Vector3.zero, Quaternion.identity)};
        }

        Transform thisAttachmentPoint = GetRandomAttachmentPoint(spaceObjectPrefab);
        Transform relativeAttachmentPoint = GetRandomAttachmentPoint(relativeSpaceObjects);

        Vector3 spaceObjectPosition = CalculateInstantiationPosition(thisAttachmentPoint, relativeAttachmentPoint);
        Quaternion spaceObjectRotation = CalculateInstantiationRotation(thisAttachmentPoint, relativeAttachmentPoint);

        GameObject instantiated = Instantiate(spaceObjectPrefab, spaceObjectPosition, spaceObjectRotation);

        Transform instantiatedAttachmentPoint = instantiated.transform.Find(thisAttachmentPoint.name);
        DestroyAttachmentPoints(instantiatedAttachmentPoint, relativeAttachmentPoint);

        return new[] {instantiated};
    }
}