/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */

using UnityEngine;
using System.Collections.Generic;

public class SceneResourceManager : SingletonComponent<SceneResourceManager>
{

    private Transform[] _uiComponents;

    [SerializeField]
    private Dictionary<string, GameObject> _cachedGameObjects = new Dictionary<string, GameObject>();

    protected override void Awake()
    {
        base.Awake();
        _uiComponents = GetObject("Canvas").GetComponentsInChildren<RectTransform>(true);
    }

    /// <summary>
    /// Finds the specified object in cache or scene.
    /// </summary>
    /// <param name="objectName">Name of the object</param>
    /// <returns>GameObject</returns>
    public GameObject GetObject(string objectName)
    {
        GameObject gameObject;
        if (_cachedGameObjects.ContainsKey(objectName))
        {
            // if cached before return it
            gameObject = _cachedGameObjects[objectName];
        }
        else
        {
            // search in unique objects
            gameObject = GameObject.Find(objectName);

            if (gameObject == null)
            {
                // search in UI objects
                foreach (RectTransform t in _uiComponents)
                {
                    if (t.name == objectName)
                    {
                        gameObject = t.gameObject;
                        break;
                    }
                }
            }

            if (gameObject != null)
                _cachedGameObjects.Add(objectName, gameObject);
            else
                Debug.LogError("[SceneResourceHolder](Cannot find objectName: " + objectName + ")");
        }

        return gameObject;
    }
}
