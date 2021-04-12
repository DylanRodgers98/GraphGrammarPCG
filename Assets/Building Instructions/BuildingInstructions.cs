using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class BuildingInstructions : MonoBehaviour
{
    private const string AttachmentPointTag = "AttachmentPoint";
    private const string EntrancePointTag = "EntrancePoint";
    private const string ExitPointTag = "ExitPoint";
    
    [SerializeField] protected GameObject spaceObjectPrefab;

    public abstract GameObject[] Build(GameObject[] relativeSpaceObjects = null);

    protected static Transform GetRandomEntrancePoint(params GameObject[] spaceObjects)
    {
        IList<Transform> attachmentPoints = GetEntrancePoints(spaceObjects);
        return GetRandomAttachmentPoint(attachmentPoints);
    }

    protected static IList<Transform> GetEntrancePoints(params GameObject[] spaceObjects)
    {
        return GetAttachmentPoints(EntrancePointTag, spaceObjects);
    }

    protected static IList<Transform> GetExitPoints(params GameObject[] spaceObjects)
    {
        return GetAttachmentPoints(ExitPointTag, spaceObjects);
    }

    private static IList<Transform> GetAttachmentPoints(string entranceOrExitPointTag, params GameObject[] spaceObjects)
    {
        IList<Transform> attachmentPoints = spaceObjects
            .SelectMany(spaceObject => spaceObject.transform.Cast<Transform>())
            .Where(child => child.CompareTag(AttachmentPointTag) || child.CompareTag(entranceOrExitPointTag))
            .ToList();

        if (attachmentPoints.Count == 0)
        {
            throw new InvalidOperationException($"No space objects in {spaceObjects} have a child object with " +
                                                $"the 'AttachmentPoint' tag or the '{entranceOrExitPointTag}' tag. " +
                                                "These tags are required to instantiate the GameObject attached to " +
                                                "an existing GameObject in the scene.");
        }

        return attachmentPoints;
    }

    protected static Transform GetRandomAttachmentPoint(IList<Transform> attachmentPoints)
    {
        return attachmentPoints[Random.Range(0, attachmentPoints.Count)];
    }

    protected static void DestroyAttachmentPoints(params Transform[] attachmentPoints)
    {
        foreach (Transform attachmentPoint in attachmentPoints)
        {
            if (attachmentPoint.CompareTag(AttachmentPointTag) || 
                attachmentPoint.CompareTag(EntrancePointTag) || 
                attachmentPoint.CompareTag(ExitPointTag))
            {
                DestroyImmediate(attachmentPoint.gameObject);
            }
        }
    }

    protected static Quaternion CalculateInstantiationRotation(Transform attachmentPoint1, Transform attachmentPoint2)
    {
        float targetY = attachmentPoint2.eulerAngles.y - 180;
        float y = targetY - attachmentPoint1.eulerAngles.y;
        return Quaternion.Euler(0, y, 0);
    }

    protected void ValidateSpaceObjectPrefab()
    {
        if (spaceObjectPrefab == null)
        {
            throw new ArgumentException("Space Object Prefab cannot be null");
        }
    }

    protected class CannotBuildException : Exception
    {
        public CannotBuildException(string message) : base(message)
        {
        }
    }
}