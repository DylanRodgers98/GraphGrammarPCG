using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GenGra
{
    public class NavMeshBaker : PostProcessor
    {
        public override void Process(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            foreach (GameObject[] spaceObjectList in generatedSpace.Values)
            {
                foreach (GameObject spaceObject in spaceObjectList)
                {
                    foreach (Transform child in spaceObject.transform)
                    {
                        NavMeshSurface navMeshSurface = child.GetComponent<NavMeshSurface>();
                        if (navMeshSurface != null)
                        {
                            navMeshSurface.BuildNavMesh();
                        }
                    }
                }
            }
        }
    }
}