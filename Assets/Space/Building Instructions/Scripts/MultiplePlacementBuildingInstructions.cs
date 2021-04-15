using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MultiplePlacementBuildingInstructions : SinglePlacementBuildingInstructions
{
    [SerializeField] private int multiplicity;

    public override GameObject[] Build(GameObject[] relativeSpaceObjects = null)
    {
        ValidateMultiplicity();

        IList<GameObject> instantiated = new List<GameObject>(multiplicity);

        for (int i = 0; i < multiplicity; i++)
        {
            GameObject[] currentInstantiated = base.Build(relativeSpaceObjects);
            foreach (GameObject obj in currentInstantiated)
            {
                instantiated.Add(obj);
            }

            IList<GameObject> newRelativeSpaceObjects = new List<GameObject>(currentInstantiated);
            if (relativeSpaceObjects != null)
            {
                foreach (GameObject relativeSpaceObject in relativeSpaceObjects)
                {
                    newRelativeSpaceObjects.Add(relativeSpaceObject);
                }
            }

            relativeSpaceObjects = newRelativeSpaceObjects.ToArray();
        }

        return instantiated.ToArray();
    }

    private void ValidateMultiplicity()
    {
        switch (multiplicity)
        {
            case 0: throw new ArgumentException("Multiplicity should be greater than 0");
            case 1:
                Debug.LogWarning($"Multiplicity for {this} is set to 1. You should use " +
                                 "an instance of SinglePlacementBuildingInstructions instead.");
                break;
        }
    }
}