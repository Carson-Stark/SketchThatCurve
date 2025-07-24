using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Functions
{
    public static float Polynomial(float x, params float[] coefficents)
    {
        float total = coefficents[0];
        for (int i = 1; i < coefficents.Length; i++)
            total += coefficents[i] * Mathf.Pow(x, i);
        return total;
    }
}
