using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GenericFunctions : MonoBehaviour
{
    public T[] ExpandArray<T>(T[] currentArray, int addative)
    {
        T[] expandedArray = new T[currentArray.Length + addative];
        int i = 0;
        while (i < currentArray.Length)
        {
            expandedArray[i] = currentArray[i];
            i++;
        }
        return (expandedArray);
    }
}
