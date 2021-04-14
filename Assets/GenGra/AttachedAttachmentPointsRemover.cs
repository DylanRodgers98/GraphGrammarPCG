using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenGra
{
    public class AttachedAttachmentPointsRemover : PostProcessor
    {
        public override void Process(GraphType missionGraph, IDictionary<string, GameObject[]> generatedSpace)
        {
            IEnumerable<GameObject> attachmentPointsToDestroy = GetAttachmentPointsByPosition()
                .Select(entry => entry.Value)
                .Where(list => list.Count > 1)
                .SelectMany(list => list);
            
            foreach (GameObject attachmentPoint in attachmentPointsToDestroy)
            {
                DestroyImmediate(attachmentPoint);
            }
        }

        private IDictionary<Vector3, IList<GameObject>> GetAttachmentPointsByPosition()
        {
            IDictionary<Vector3, IList<GameObject>> attachmentPointsByPosition = new Dictionary<Vector3, IList<GameObject>>();
            GetAttachmentPointsByPosition(attachmentPointsByPosition, BuildingInstructions.AttachmentPointTag);
            GetAttachmentPointsByPosition(attachmentPointsByPosition, BuildingInstructions.EntrancePointTag);
            GetAttachmentPointsByPosition(attachmentPointsByPosition, BuildingInstructions.ExitPointTag);
            return attachmentPointsByPosition;
        }

        private void GetAttachmentPointsByPosition(IDictionary<Vector3, IList<GameObject>> attachmentPointsByPosition, 
            string attachmentPointTag)
        {
            foreach (GameObject attachmentPoint in GameObject.FindGameObjectsWithTag(attachmentPointTag))
            {
                Vector3 position = attachmentPoint.transform.position;
                if (!attachmentPointsByPosition.ContainsKey(position))
                {
                    attachmentPointsByPosition[position] = new List<GameObject>();
                }

                attachmentPointsByPosition[position].Add(attachmentPoint);
            }
        }
    }
}