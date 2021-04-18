using System.Collections.Generic;
using UnityEngine;

namespace GenGra
{
    public abstract class PostProcessor : MonoBehaviour
    {
        public abstract void Process(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace);
    }
}