using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReplacementBuildingInstructions : BuildingInstructions
{
    public override GameObject[] Build(GameObject[] spaceObjectsToReplace = null, bool checkForOverlap = true)
    {
        ValidateSpaceObjectsToReplace(spaceObjectsToReplace);
        ValidateSpaceObjectVariants();

        IList<GameObject> instantiatedSpaceObjects = new List<GameObject>();
        GameObject spaceObjectPrefab = GetRandomSpaceObject();
        
        foreach (GameObject spaceObjectToReplace in spaceObjectsToReplace)
        {
            GameObject instantiated = ReplaceSpaceObject(spaceObjectToReplace, spaceObjectPrefab);
            if (DoesInstantiatedOverlapOtherSpaceObjects(instantiated))
            {
                DestroyImmediate(instantiated);
            }
            else
            {
                instantiatedSpaceObjects.Add(instantiated);
            }
        }

        return instantiatedSpaceObjects.ToArray();
    }

    private static void ValidateSpaceObjectsToReplace(GameObject[] spaceObjectsToReplace)
    {
        if (spaceObjectsToReplace == null || spaceObjectsToReplace.Length == 0)
        {
            throw new ArgumentException("Space Objects To Replace array cannot be null or empty");
        }
    }
    
    private static GameObject ReplaceSpaceObject(GameObject spaceObjectToReplace, GameObject prefabToInstantiate)
    {
        Vector3 position = spaceObjectToReplace.transform.position;
        Quaternion rotation = spaceObjectToReplace.transform.rotation;
        spaceObjectToReplace.SetActive(false);
        return Instantiate(prefabToInstantiate, position, rotation);
    }
}
