using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class BuildingInstructions : MonoBehaviour
{
    [SerializeField] protected GameObject spaceObjectPrefab;

    public abstract GameObject[] Build(GameObject[] relativeSpaceObjects = null);

    protected void ValidateSpaceObjectPrefab()
    {
        if (spaceObjectPrefab == null)
        {
            throw new ArgumentException("Space Object Prefab cannot be null");
        }
    }

    protected Transform GetRandomAttachmentPoint(params GameObject[] spaceObjects)
    {
        Transform[] attachmentPoints = spaceObjects
            .SelectMany(spaceObject => spaceObject.transform.Cast<Transform>())
            .Where(child => child.CompareTag("AttachmentPoint"))
            .ToArray();

        if (attachmentPoints.Length == 0)
        {
            throw new InvalidOperationException($"No space objects in {spaceObjects} have a child object with " +
                                                "the 'AttachmentPoint' tag. This tag is required to instantiate one " +
                                                "the GameObjects attached to an existing GameObject in the scene.");
        }

        return attachmentPoints[Random.Range(0, attachmentPoints.Length)];
    }
}
