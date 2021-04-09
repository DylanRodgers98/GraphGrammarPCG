using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleBuildingInstructions : BuildingInstructions
{
    public override void Build()
    {
        Debug.Log($"[Single Building Instructions] Space Object: {spaceObject}");
    }
}
