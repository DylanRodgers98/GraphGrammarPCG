using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class BuildingInstructions : MonoBehaviour
{
    [SerializeField] protected GameObject spaceObjectPrefab;

    public abstract GameObject[] Build(GameObject[] relativeSpaceObjects = null);

    protected static Transform GetRandomAttachmentPoint(params GameObject[] spaceObjects)
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

    protected static void DestroyAttachmentPoints(params Transform[] attachmentPoints)
    {
        foreach (Transform attachmentPoint in attachmentPoints)
        {
            DestroyImmediate(attachmentPoint.gameObject);
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
}