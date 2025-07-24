using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GraphSubmission
{
    public Point[] graph;
    public float directScore;
    public float slopeScore;
    public float totalScore;
    public int scoreChange;
    public int timeSpent;
}

[System.Serializable]
public class Point
{
    public float x;
    public float y;

    public Point(float X, float Y)
    {
        x = X;
        y = Y;
    }
}
