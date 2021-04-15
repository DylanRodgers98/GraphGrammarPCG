using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInstructionsFactory : MonoBehaviour
{
    /*
     * See doc comment on struct BuildingInstructionsHolder about
     * the necessity for both a public Array and a private IDictionary
     */
    [SerializeField] private BuildingInstructionsHolder[] buildingInstructionsByMissionSymbol;
    private IDictionary<string, GameObject> buildingInstructionsByMissionSymbolDict;

    private IDictionary<string, GameObject> BuildingInstructionsByMissionSymbol
    {
        get
        {
            if (buildingInstructionsByMissionSymbolDict == null)
            {
                buildingInstructionsByMissionSymbolDict = new Dictionary<string, GameObject>();
                foreach (BuildingInstructionsHolder biHolder in buildingInstructionsByMissionSymbol)
                {
                    buildingInstructionsByMissionSymbolDict[biHolder.MissionSymbol] =
                        biHolder.BuildingInstructionsPrefab;
                }
            }

            return buildingInstructionsByMissionSymbolDict;
        }
    }

    public GameObject[] Build(string missionSymbol, GameObject[] relativeSpaceObjects = null)
    {
        try
        {
            GameObject biPrefab = BuildingInstructionsByMissionSymbol[missionSymbol];
            BuildingInstructions buildingInstructions = biPrefab.GetComponent<BuildingInstructions>();
            if (buildingInstructions == null)
            {
                throw new InvalidOperationException("No BuildingInstructions component found attached to " +
                                                    $"{biPrefab}. Please check validity of this prefab.");
            }

            return buildingInstructions.Build(relativeSpaceObjects);
        }
        catch (KeyNotFoundException)
        {
            throw new InvalidOperationException("No building instructions prefab found for mission symbol " +
                                                $"'{missionSymbol}'. Please check validity of Building " +
                                                "Instructions By Mission Symbol array.");
        }
    }

    /**
     * This struct is a workaround to allow GameObjects to be mapped to a string in the Unity editor in a
     * similar vein to using a Dictionary, because IDictionary is not serializable by the Unity engine.
     */
    [Serializable]
    private class BuildingInstructionsHolder
    {
        [SerializeField] private string missionSymbol;
        [SerializeField] private GameObject buildingInstructionsPrefab;

        public string MissionSymbol => missionSymbol;

        public GameObject BuildingInstructionsPrefab => buildingInstructionsPrefab;
    }
}