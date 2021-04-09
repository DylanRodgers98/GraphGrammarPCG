using System;
using UnityEngine;

public class SingleBuildingInstructions : BuildingInstructions
{
    public override GameObject[] Build(GameObject[] relativeSpaceObjects = null)
    {
        ValidateSpaceObjectPrefab();
        Debug.Log($"[Single Building Instructions] Space Object: {spaceObjectPrefab}");
        if (relativeSpaceObjects?.Length > 0) return Array.Empty<GameObject>();
        GameObject instantiatedSpaceObject = Instantiate(spaceObjectPrefab, Vector3.zero, Quaternion.identity);
        return new []{instantiatedSpaceObject};
    }
}
