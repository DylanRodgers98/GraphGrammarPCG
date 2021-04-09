using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingInstructions : MonoBehaviour
{
    [SerializeField] protected GameObject spaceObject;

    public abstract void Build();
}
