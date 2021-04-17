using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class BuildingInstructions : MonoBehaviour
{
    public const string AttachmentPointTag = "AttachmentPoint";
    public const string EntrancePointTag = "EntrancePoint";
    public const string ExitPointTag = "ExitPoint";

    [SerializeField] private WeightedSpaceObject[] spaceObjectVariants;

    public abstract GameObject[] Build(GameObject[] relativeSpaceObjects = null);

    protected static Transform GetRandomEntrancePoint(params GameObject[] spaceObjects)
    {
        IList<Transform> attachmentPoints = GetEntrancePoints(spaceObjects);
        return GetRandomAttachmentPoint(attachmentPoints);
    }

    protected static IList<Transform> GetEntrancePoints(params GameObject[] spaceObjects)
    {
        IList<Transform> entrancePoints = GetAttachmentPoints(EntrancePointTag, spaceObjects);

        if (entrancePoints.Count == 0)
        {
            throw new InvalidOperationException($"No space objects in {spaceObjects} have a child object with " +
                                                $"the 'AttachmentPoint' tag or the '{EntrancePointTag}' tag. These " +
                                                "tags are required to instantiate the GameObject attached to an " +
                                                "existing GameObject in the scene.");
        }

        return entrancePoints;
    }

    protected static IList<Transform> GetExitPoints(params GameObject[] spaceObjects)
    {
        return GetAttachmentPoints(ExitPointTag, spaceObjects);
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
        GameObject spaceObjectPrefab = GetRandomSpaceObject();
        GameObject instantiated = Instantiate(spaceObjectPrefab, position, rotation);
        Transform instantiatedAttachmentPoint = instantiated.transform.Find(attachmentPoint.name);
        Vector3 translation = instantiated.transform.position - instantiatedAttachmentPoint.position;
        instantiated.transform.Translate(translation, Space.World);

        if (DoesInstantiatedOverlapOtherSpaceObjects(instantiated))
        {
            DestroyImmediate(instantiated);
            return null;
        }

        DestroyImmediate(instantiatedAttachmentPoint.gameObject);
        return instantiated;
    }

    protected GameObject GetRandomSpaceObject()
    {
        if (spaceObjectVariants.Length == 1)
        {
            return spaceObjectVariants[0].SpaceObject;
        }
        
        int totalWeighting = 0;
        foreach (WeightedSpaceObject spaceObjectVariant in spaceObjectVariants)
        {
            if (spaceObjectVariant.Weighting < 1)
            {
                spaceObjectVariant.Weighting = 1;
            }

            totalWeighting += spaceObjectVariant.Weighting;
        }
        
        int desiredWeighting = Random.Range(1, totalWeighting);
        int currentWeighting = 0;
        
        foreach (WeightedSpaceObject spaceObjectVariant in spaceObjectVariants)
        {
            currentWeighting += spaceObjectVariant.Weighting;
            if (desiredWeighting <= currentWeighting)
            {
                return spaceObjectVariant.SpaceObject;
            }
        }

        throw new InvalidOperationException($"Could not determine space object for weighting {desiredWeighting}");
    }

    protected void ValidateSpaceObjectPrefab()
    {
        if (spaceObjectVariants == null || spaceObjectVariants.Length == 0)
        {
            throw new ArgumentException("Space Object Variants array cannot be null or empty");
        }
    }

    private static IList<Transform> GetAttachmentPoints(string entranceOrExitPointTag, params GameObject[] spaceObjects)
    {
        return spaceObjects
            .SelectMany(spaceObject => spaceObject.transform.Cast<Transform>())
            .Where(child => child.CompareTag(AttachmentPointTag) || child.CompareTag(entranceOrExitPointTag))
            .ToList();
    }

    private static bool DoesInstantiatedOverlapOtherSpaceObjects(GameObject instantiated)
    {
        Collider[] colliders = new Collider[2];
        int numberOfObjectsAtInstantiatedLocation = Physics.OverlapBoxNonAlloc(instantiated.transform.position,
            instantiated.transform.localScale / 2 * 0.99f, colliders);
        return numberOfObjectsAtInstantiatedLocation > 1;
    }

    protected class CannotBuildException : Exception
    {
        public CannotBuildException(string message) : base(message)
        {
        }
    }

    [Serializable]
    private class WeightedSpaceObject
    {
        [SerializeField] private GameObject spaceObject;
        [SerializeField] private int weighting = 1;

        public GameObject SpaceObject => spaceObject;

        public int Weighting
        {
            get => weighting;
            set => weighting = value;
        }
    }
}