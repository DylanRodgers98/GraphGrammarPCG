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

    protected static Quaternion CalculateInstantiationRotation(Transform attachmentPoint1, Transform attachmentPoint2)
    {
        float targetY = attachmentPoint2.eulerAngles.y - 180;
        float y = targetY - attachmentPoint1.eulerAngles.y;
        return Quaternion.Euler(0, y, 0);
    }

    protected GameObject InstantiateSpaceObject(Vector3 position, Quaternion rotation, Transform attachmentPoint)
    {
        GameObject instantiated = Instantiate(spaceObjectPrefab, position, rotation);
        Transform instantiatedAttachmentPoint = instantiated.transform.Find(attachmentPoint.name);
        Vector3 translation = instantiated.transform.position - instantiatedAttachmentPoint.position;
        instantiated.transform.Translate(translation, Space.World);
        
        if (DoesInstantiatedOverlapOtherSpaceObjects(instantiated))
        {
            DestroyImmediate(instantiated);
            return null;
        }
        
        DestroyImmediate(instantiatedAttachmentPoint);
        return instantiated;
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
    
    private static bool DoesInstantiatedOverlapOtherSpaceObjects(GameObject instantiated)
    {
        Collider[] colliders = new Collider[2];
        int numberOfObjectsAtInstantiatedLocation = Physics.OverlapBoxNonAlloc(instantiated.transform.position, 
            instantiated.transform.localScale / 2 * 0.99f, colliders);
        return numberOfObjectsAtInstantiatedLocation > 1;
    }
}