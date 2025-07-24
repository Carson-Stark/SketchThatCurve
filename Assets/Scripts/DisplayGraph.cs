using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayGraph : MonoBehaviour
{
    public LineRenderer lineMaker;
    public float drawSpeed;

    LineRenderer currRenderer;
    Vector3[] corners;
    float wait2draw = 2;

    List<Renderer> rends = new List<Renderer>();

    void Start()
    {
        corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
    }

    Vector2 graphToWorld(Point graphPoint, float sizex, float sizey)
    {
        Vector2 worldPos = new Vector2();
        float scalex = (corners[2].x - corners[0].x) / (sizex * 2);
        float scaley = (corners[1].y - corners[0].y) / (sizey * 2);
        worldPos.x = corners[0].x + (graphPoint.x + sizex) * scalex;
        worldPos.y = corners[0].y + (graphPoint.y + sizey) * scaley;
        return worldPos;
    }

    public IEnumerator drawGraph(Point[] coords, float sizex, float sizey, Color color)
    {
        yield return new WaitForSeconds(0.1f);
        Start();

        bool drawing = false;

        foreach (Point coord in coords)
        {
            if (coord.y == Mathf.Infinity && drawing)
                drawing = false;
            else if (coord.y < Mathf.Infinity && !drawing)
            {
                drawing = true;
                currRenderer = Instantiate(lineMaker, transform).GetComponent<LineRenderer>();
                rends.Add(currRenderer);
                currRenderer.positionCount--;
                currRenderer.startColor = color;
                currRenderer.endColor = color;
            }

            if (drawing)
            {
                //yield return new WaitForSeconds(drawSpeed);
                currRenderer.positionCount++;
                currRenderer.SetPosition(currRenderer.positionCount - 1, graphToWorld(coord, sizex, sizey));
            }
        }
    }

    public delegate float function(float x);

    public IEnumerator drawGraph(function f, float sizex, float sizey, int resolution, Color color, bool startHidden = false)
    {
        yield return new WaitForSeconds(0.1f);
        Start();

        float step = sizex / (float)resolution;
        bool drawing = false;

        for (float x = -sizex; x < sizex; x += step)
        {
            Vector2 worldpos = graphToWorld(new Point(x, f(x)), sizex, sizey);
            if (!validWorldPos(worldpos) && drawing)
                drawing = false;
            else if (validWorldPos(worldpos) && !drawing)
            {
                drawing = true;
                currRenderer = Instantiate(lineMaker, transform).GetComponent<LineRenderer>();
                if (startHidden)
                    currRenderer.enabled = false;
                rends.Add(currRenderer);
                currRenderer.positionCount--;
                currRenderer.startColor = color;
                currRenderer.endColor = color;
            }

            if (drawing)
            {
                //yield return new WaitForSeconds(drawSpeed);
                currRenderer.positionCount++;
                currRenderer.SetPosition(currRenderer.positionCount - 1, worldpos);
            }
        }
    }

    bool validWorldPos(Vector3 pos)
    {
        return pos.y != float.NaN && pos.y < corners[1].y && pos.y > corners[0].y &&
                pos.x > corners[0].x && pos.x < corners[2].x;
    }

    public void resetGraph()
    {
        //remove renderers
        foreach (LineRenderer r in rends)
        {
            Destroy(r.gameObject);
        }
        rends.Clear();
    }

    public void hideGraph()
    {
        foreach (LineRenderer r in rends)
        {
            r.enabled = false;
        }
    }

    public void revealGraph()
    {
        foreach (LineRenderer r in rends)
        {
            r.enabled = true;
        }
    }
}
