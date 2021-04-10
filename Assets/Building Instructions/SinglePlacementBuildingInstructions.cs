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
        Transform relativeAttachmentPoint = GetRandomAttachmentPoint(relativeSpaceObjects);

        Quaternion spaceObjectRotation = CalculateInstantiationRotation(thisAttachmentPoint, relativeAttachmentPoint);

        GameObject instantiated = Instantiate(spaceObjectPrefab, relativeAttachmentPoint.position, spaceObjectRotation);
        Transform instantiatedAttachmentPoint = instantiated.transform.Find(thisAttachmentPoint.name);
        Vector3 translation = instantiated.transform.position - instantiatedAttachmentPoint.position;
        instantiated.transform.Translate(translation, Space.World);
        
        DestroyAttachmentPoints(instantiatedAttachmentPoint, relativeAttachmentPoint);

        return new[] {instantiated};
    }
}