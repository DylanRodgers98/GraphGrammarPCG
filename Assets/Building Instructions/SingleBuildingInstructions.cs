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

        /*
         * Calculate position of space object to be instantiated
         */
        Vector3 thisObjectPosition = spaceObjectPrefab.transform.position;
        Vector3 thisAttachmentPointVectorFromCenter = thisAttachmentPoint.position - thisObjectPosition;

        Vector3 relativeObjectPosition = relativeAttachmentPoint.parent.position;
        Vector3 relativeAttachmentPointVectorFromCenter = relativeAttachmentPoint.position - relativeObjectPosition;

        Vector3 vectorBetweenCenters = thisAttachmentPointVectorFromCenter + relativeAttachmentPointVectorFromCenter;

        Vector3 spaceObjectPosition = relativeObjectPosition + vectorBetweenCenters;

        // calculate rotation of space object to be instantiated
        float y = thisAttachmentPoint.eulerAngles.y - relativeAttachmentPoint.eulerAngles.y - 180;
        Quaternion spaceObjectRotation = Quaternion.Euler(0, y, 0);

        GameObject instantiated = Instantiate(spaceObjectPrefab, spaceObjectPosition, spaceObjectRotation);

        // destroy used attachment points
        Transform instantiatedAttachmentPoint = instantiated.transform.Find(thisAttachmentPoint.name);
        Destroy(instantiatedAttachmentPoint.gameObject);
        Destroy(relativeAttachmentPoint.gameObject);

        return new[] {instantiated};
    }
}