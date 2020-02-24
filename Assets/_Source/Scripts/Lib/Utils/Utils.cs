/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;

public class Utils
{
    #region RESOURCES
    public static T[] GetArrayOfAssetsAtPath<T>(string path) where T : Object
    {
        return Resources.LoadAll<T>(path);
    }
    public static T GetAssetAtPath<T>(string path) where T : Object
    {
        try
        {
            return (T)Resources.Load(path);
        }
        catch
        {
            Debug.LogWarning("Is no asset named \"" + path + "\"");
            return null;
        }
    }
    #endregion
    #region INSTANTIATE
    public static GameObject CreateGameObject(string name, Vector3 position = default(Vector3), Transform parent = null)
    {
        var obj = new GameObject(name);
        obj.transform.parent = parent;
        obj.transform.localPosition = position;
        return obj;
    }
    public static GameObject InstantiateGameObject(GameObject obj, Vector3 position = default(Vector3), Transform parent = null)
    {
        if (obj == null)
        {
            Debug.LogWarning("GameObject is null");
            return null;
        }
        var createdObject = Object.Instantiate(obj) as GameObject;
        createdObject.transform.SetParent(parent);
        createdObject.transform.localPosition = position;
        return createdObject;
    }
    public static GameObject InstantiatePrefab(string name, Vector3 position = default(Vector3), Transform parent = null)
    {
        GameObject go = GetAssetAtPath<GameObject>("Prefabs/" + name);
        if (go == null)
        {
            Debug.LogWarning("Prefab " + name + " was not find.");
            return null;
        }
        return InstantiateGameObject(go, position, parent);
    }
    #endregion
    #region OTHER

    public static T GetRandomEnumValue<T>()
    {
        var v = Enum.GetValues(typeof(T));
        return (T)v.GetValue(Random.Range(0,v.Length));
    }

    public static string GetPersistentFilePath(string fileName)
    {
        string filePath = "";
#if UNITY_IPHONE
		    filePath = Application.persistentDataPath + "/" + fileName;
#elif UNITY_EDITOR
        filePath = Application.persistentDataPath + "/" + fileName;
#elif UNITY_ANDROID
		    filePath = Application.persistentDataPath + "/" + fileName;
#else
		    filePath = Application.dataPath + "/" + fileName;
#endif
        return filePath;
    }
    
    public static bool isPointerOverUnityUI()
    {
        bool isOverUI = false;
        if (EventSystem.current.currentInputModule is StandaloneInputModule)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {                     //traverse trough the touchs and find if one is touching
                Touch touch = Input.touches[i];
                if (touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId))
                    {
                        isOverUI = true;
                        break;
                    }
                }
            }
        }
        else
        {
            isOverUI = EventSystem.current.IsPointerOverGameObject();   //there is no touch only the mouse
        }
        return isOverUI;
    }
    public static Vector3 GetCursorPos()
    {
#if UNITY_EDITOR
        return Input.mousePosition;
#else
        if (Input.touchCount > 0)
        {
            return Input.touches[0].position;
        }
        else
        {
            return new Vector3();
        }
#endif
    }
    #endregion
}

public static class Extentions
{
    /// <summary>
    /// Removes the childrens of this Transform object.
    /// </summary>
    /// <param name="parent">Parent Transfotm object</param>
    public static void RemoveChildren(this Transform parent)
    {
        foreach (Transform child in parent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    /// <summary>
    /// 999999999 -> 999,999,999
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToFormat(this int value)
    {
        return String.Format("{0:n0}", value);
    }

    public static T[] Concat<T>(this T[] x, T[] y)
    {
        if (x == null) throw new ArgumentNullException("x");
        if (y == null) throw new ArgumentNullException("y");
        int oldLen = x.Length;
        Array.Resize<T>(ref x, x.Length + y.Length);
        Array.Copy(y, 0, x, oldLen, y.Length);
        return x;
    }

    /// <summary>
    /// Also sets the layer info of the childs of the transform object (using Recursion).
    /// </summary>
    public static void SetLayerRecursively(this GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }

#region EXTENSION METHODS
    //  ==================== POSITION ====================
    public static void SetPositionX(this Transform t, float value)
    {
        t.position = new Vector3(value, t.position.y, t.position.z);
    }
    public static void SetPositionY(this Transform t, float value)
    {
        t.position = new Vector3(t.position.x, value, t.position.z);
    }
    public static void SetPositionZ(this Transform t, float value)
    {
        t.position = new Vector3(t.position.x, t.position.y, value);
    }

    //  ================= LOCAL POSITION =================
    public static void SetLocalPositionX(this Transform t, float value)
    {
        t.localPosition = new Vector3(value, t.localPosition.y, t.localPosition.z);
    }
    public static void SetLocalPositionY(this Transform t, float value)
    {
        t.localPosition = new Vector3(t.localPosition.x, value, t.localPosition.z);
    }
    public static void SetLocalPositionZ(this Transform t, float value)
    {
        t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, value);
    }

    //  ==================== ROTATION ====================
    public static void SetEulerX(this Transform t, float value)
    {
        t.rotation = Quaternion.Euler(new Vector3(value, t.rotation.eulerAngles.y, t.rotation.eulerAngles.z));
    }
    public static void SetEulerY(this Transform t, float value)
    {
        t.rotation = Quaternion.Euler(new Vector3(t.rotation.eulerAngles.x, value, t.rotation.eulerAngles.z));
    }
    public static void SetEulerZ(this Transform t, float value)
    {
        t.rotation = Quaternion.Euler(new Vector3(t.rotation.eulerAngles.x, t.rotation.eulerAngles.y, value));
    }
    //  ==================== List<T> ====================
    public static T PopAt<T>(this List<T> list, int index)
    {
        T r = list[index];
        list.RemoveAt(index);
        return r;
    }

    // ==================== GRAPHIC / RENDERER ====================

    public static Material SetSaturationMaterial(this Graphic graph, float value)
    {
        Material satMat;
        if (graph.material == null || graph.material.name != "SaturatedSprite")
        {
            satMat = GameObject.Instantiate<Material>(Utils.GetAssetAtPath<Material>("Materials/SaturatedSprite"));
            graph.material = satMat;
        }
        else
        {
            satMat = graph.material;
        }

        satMat.SetFloat("_Saturation", value);
        return graph.material;
    }
    public static Material SetSaturationMaterial(this Renderer renderer, float value)
    {
        Material satMat;
        if (renderer.material == null || renderer.material.name != "SaturatedSprite")
        {
            satMat = GameObject.Instantiate<Material>(Utils.GetAssetAtPath<Material>("Materials/SaturatedSprite"));
            renderer.material = satMat;
        }
        else
        {
            satMat = renderer.material;
        }

        satMat.SetFloat("_Saturation", value);
        return renderer.material;
    }

    public static void SetGrayScaleInChildrens(this GameObject gameObject, float value)
    {
        List<Graphic> childrenGraphicList = gameObject.GetComponentsInChildren<Graphic>().ToList();
        List<Renderer> childrenRendererList = gameObject.GetComponentsInChildren<Renderer>().ToList();

        foreach (Graphic g in childrenGraphicList)
        {
            SetSaturationMaterial(g, value);
        }

        foreach (Renderer r in childrenRendererList)
        {
            SetSaturationMaterial(r, value);
        }
    }

    public static void SetAlpha(this GameObject gameObject, float value)
    {
        Graphic[] graphics = gameObject.GetComponentsInChildren<Graphic>();

        foreach (Graphic gr in graphics)
        {
            gr.canvasRenderer.SetAlpha(value);
        }
    }

    // ==================== String uni encoding ====================

    public static string ConvertEncodedStringToNormal(this string text)
    {
        //< ->  & lt;
        //> ->  & gt;
        //" ->	&quot;
        //' ->	&apos;
        //& ->  & amp;
        text = text.Replace("&lt;", "<");
        text = text.Replace("&gt;", ">");
        text = text.Replace("&quot;", "\"");
        text = text.Replace("&apos;", "'");
        text = text.Replace("&amp;", "&");

        return text;
    }
#endregion
}