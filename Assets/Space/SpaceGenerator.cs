using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Space
{
    public class SpaceGenerator : MonoBehaviour
    {
        [Serializable]
        private struct SpaceObjectByMissionSymbol
        {
            [SerializeField] private string missionSymbol;
            [SerializeField] private GameObject spaceObject;
        }

        [SerializeField] private SpaceObjectByMissionSymbol[] spaceObjectsByMissionSymbol;

        void Start()
        {

        }
    }
}
