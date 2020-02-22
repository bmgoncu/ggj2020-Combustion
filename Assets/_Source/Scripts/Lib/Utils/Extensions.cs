using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;

public static class Extensions
{
    #region List
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        for (var i = 0; i < list.Count; i++)
            list.Swap(i, rng.Next(i, list.Count));
    }
    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }
    public static T RandomItem<T>(this IList<T> list)
    {
        if (list.Count == 0)
            return default(T);
        if (list.Count == 1)
            return list[0];
        return list[(rng.Next(0, list.Count))];
    }
    public static decimal Median(this IEnumerable<int> source)
    {
        // Create a copy of the input, and sort the copy
        int[] temp = source.ToArray();
        Array.Sort(temp);

        int count = temp.Length;
        if (count == 0)
        {
            return 0;
        }
        else if (count % 2 == 0)
        {
            // count is even, average two middle elements
            int a = temp[count / 2 - 1];
            int b = temp[count / 2];
            return (a + b) / 2m;
        }
        else
        {
            // count is odd, return the middle element
            return temp[count / 2];
        }
    }
    #endregion

    #region GameObject 
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
    #endregion
}