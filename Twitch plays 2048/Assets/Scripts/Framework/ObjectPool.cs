using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    #region Properties
    public GameObject RefBlueprint { get { return refBlueprint; } }
    #endregion

    #region Settings
    [Header("Settings")]
    [SerializeField]
    private GameObject refBlueprint;
    [SerializeField]
    private int initialSize = 10;
    [SerializeField]
    private int maxSize = 30;
    [SerializeField]
    private int creationBatch = 5;
    #endregion

    private List<GameObject> objects = new List<GameObject>();

    #region MonoBehaviour
    private void Start()
    {
        create(initialSize);
    }
    #endregion

    public GameObject Get()
    {
        GameObject toReturn = null;
        // Find object
        foreach (var obj in objects)
        {
            if (!obj.activeSelf)
            {
                toReturn = obj;
                break;
            }
        }

        if (!toReturn)
        {
            // Attempt to create
            toReturn = create(creationBatch);
        }

        if (toReturn)
        {
            toReturn.SetActive(true);
        }
        return toReturn;
    }

    public void ResetAll()
    {
        foreach (var o in objects)
        {
            o.transform.SetParent(transform, true);
            o.SetActive(false);
        }
    }

    #region Helper Functions
    private GameObject create(int count)
    {
        if (objects.Count >= maxSize)
            return null;
        else
        {
            var delta = maxSize - objects.Count;
            count = delta < count ? delta : count;
        }

        // Create
        GameObject first = null;
        for (int i = 0; i < count; ++i)
        {
            // Create object
            var newObj = Instantiate(refBlueprint);
            newObj.transform.SetParent(transform, true);

            // Inactive object
            newObj.SetActive(false);

            // Add to list
            objects.Add(newObj);

            if (!first)
                first = newObj;
        }
        return first;
    }
    #endregion
}
