using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static T GetRandomElement<T>(List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static T GetRandomElement<T>(T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }
}
