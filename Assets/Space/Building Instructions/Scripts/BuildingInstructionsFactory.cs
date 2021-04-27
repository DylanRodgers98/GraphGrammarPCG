using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        BuildingInstructions buildingInstructions = GetRandomBuildingInstructions(missionSymbol);
        return buildingInstructions.Build(relativeSpaceObjects);
    }

    private BuildingInstructions GetRandomBuildingInstructions(string missionSymbol)
    {
        BuildingInstructions[] buildingInstructionsArray = GetBuildingInstructions(missionSymbol);

        if (buildingInstructionsArray.Length == 1)
        {
            return buildingInstructionsArray[0];
        }
        
        int totalWeighting = 0;
        foreach (BuildingInstructions buildingInstructions in buildingInstructionsArray)
        {
            if (buildingInstructions.Weighting < 1)
            {
                buildingInstructions.Weighting = 1;
            }

            totalWeighting += buildingInstructions.Weighting;
        }
        
        int desiredWeighting = Random.Range(1, totalWeighting + 1);
        int currentWeighting = 0;
        
        foreach (BuildingInstructions buildingInstructions in buildingInstructionsArray)
        {
            currentWeighting += buildingInstructions.Weighting;
            if (desiredWeighting <= currentWeighting)
            {
                return buildingInstructions;
            }
        }

        throw new InvalidOperationException("Could not determine building instructions " +
                                            $"for weighting {desiredWeighting}");
    }

    private BuildingInstructions[] GetBuildingInstructions(string missionSymbol)
    {
        GameObject biPrefab;
        try
        {
            biPrefab = BuildingInstructionsByMissionSymbol[missionSymbol];
        }
        catch (KeyNotFoundException)
        {
            throw new InvalidOperationException("No building instructions prefab found for mission symbol " +
                                                $"'{missionSymbol}'. Please check validity of Building " +
                                                "Instructions By Mission Symbol array.");
        }
        
        BuildingInstructions[] buildingInstructionsArray = biPrefab.GetComponents<BuildingInstructions>();
        if (buildingInstructionsArray == null || buildingInstructionsArray.Length == 0)
        {
            throw new InvalidOperationException("No BuildingInstructions component found attached to " +
                                                $"{biPrefab}. Please check validity of this prefab.");
        }

        return buildingInstructionsArray;
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