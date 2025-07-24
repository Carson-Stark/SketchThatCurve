using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Graphing : MonoBehaviour
{
    public GameObject LineMakerPrefab;
    public Text[] bounds_txt;
    private LineRenderer currRend;
    private List<LineRenderer> rends = new List<LineRenderer>();
    private Vector3[] corners;
    private float[] yvals;
    public int resolution;

    float columnSize;
    private int lastCol;
    private int lastLineCol;
    bool drawing;
    bool linepaused;

    // Start is called before the first frame update
    void Start()
    {
        corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);

        columnSize = (corners[3].x - corners[0].x) / resolution;
        lastCol = resolution;

        resetGraph();
    }

    public void initializeBounds(string sizex, string sizey)
    {
        bounds_txt[0].text = sizex;
        bounds_txt[1].text = "-" + sizex;
        bounds_txt[2].text = sizey;
        bounds_txt[3].text = "-" + sizey;
    }

    public void resetGraph()
    {
        //default column value is inf
        yvals = new float[resolution];
        for (int i = 0; i < yvals.Length; i++)
        {
            yvals[i] = Mathf.Infinity;
        }

        //remove renderers
        foreach (LineRenderer r in rends)
        {
            Destroy(r.gameObject);
        }
        rends.Clear();
    }

    int posToColumn(float x)
    {
        return (int)((x - corners[0].x) / columnSize);
    }

    float columnToPos(int c)
    {
        return c * columnSize + corners[0].x;
    }

    bool continuePausedLine(int thisCol)
    {
        if (lastLineCol < resolution - 1 && yvals[lastLineCol + 1] == Mathf.Infinity)
            //needs to continue from right
            return thisCol > lastLineCol;
        if (lastLineCol > 0 && yvals[lastLineCol - 1] == Mathf.Infinity)
            //needs to continue from left
            return thisCol < lastLineCol;
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currPoint.z = 0;

        int thisCol = posToColumn(currPoint.x);

        if (Input.GetMouseButton(0) && checkBounds(currPoint))
        {
            //if we've left the previous column and this column hasn't been drawed on
            if (yvals[thisCol] == Mathf.Infinity && thisCol != lastCol)
            {
                if (linepaused && continuePausedLine(thisCol))
                    linepaused = false;

                if (!drawing || linepaused) //we weren't drawing so start new line
                    NewLine(currPoint);
                else if (!linepaused || continuePausedLine(thisCol))
                {
                    //fill in missed points
                    if (lastCol < thisCol - 1 && lastCol >= 0)
                    {
                        for (int c = lastCol + 1; c <= thisCol; c++)
                        {
                            float cx = columnToPos(c);
                            float cy = getPointOnLine(new Vector3(columnToPos(lastCol) + columnSize / 2, yvals[lastCol]), currPoint, cx);
                            yvals[c] = cy;
                            currRend.positionCount += 1;
                            currRend.SetPosition(currRend.positionCount - 1, new Vector3(cx, cy, 0));
                        }
                    }
                    else if (lastCol > thisCol + 1 && lastCol < resolution)
                    {
                        for (int c = lastCol - 1; c > thisCol; c--)
                        {
                            float cx = columnToPos(c);
                            float cy = getPointOnLine(new Vector3(columnToPos(lastCol) + columnSize / 2, yvals[lastCol]), currPoint, cx);
                            yvals[c] = cy;
                            currRend.positionCount += 1;
                            currRend.SetPosition(currRend.positionCount - 1, new Vector3(cx, cy, 0));
                        }
                    }

                    //now add current point
                    currRend.positionCount += 1;
                    yvals[thisCol] = currPoint.y;
                    currRend.SetPosition(currRend.positionCount - 1, currPoint);
                }

                lastLineCol = thisCol;
            }
            else if (lastCol != thisCol)
            {
                //we've already drawn in this column
                linepaused = true;
            }

            lastCol = thisCol;
        }

        //end line when we pick mouse up
        if (Input.GetMouseButtonUp(0))
            drawing = false;
    }

    private float getPointOnLine(Vector3 start, Vector3 end, float x)
    {
        float m = (end.y - start.y) / (end.x - start.x);
        float b = end.y - m * end.x;
        return m * x + b;
    }

    private bool checkBounds(Vector3 currPoint)
    {
        int c = posToColumn(currPoint.x);
        return c >= 0 && c < resolution && currPoint.y >= corners[0].y && currPoint.y <= corners[1].y;
    }

    private void NewLine(Vector3 currPoint)
    {
        currRend = Instantiate(LineMakerPrefab.GetComponent<LineRenderer>(),
            GetComponent<RectTransform>().position, Quaternion.identity, transform);
        currRend.SetPosition(0, currPoint);
        yvals[posToColumn(currPoint.x)] = currPoint.y;
        drawing = true;
        linepaused = false;
        rends.Add(currRend);
    }

    public Point[] getGraphCoords(float sizex, float sizey)
    {
        Point[] coords = new Point[yvals.Length];
        float scalex = sizex * 2 / (corners[2].x - corners[0].x);
        float scaley = sizey * 2 / (corners[1].y - corners[0].y);

        for (int c = 0; c < yvals.Length; c++)
        {
            Point coord = new Point((columnToPos(c) - corners[0].x) * scalex - sizex, (yvals[c] - corners[0].y) * scaley - sizey);
            coords[c] = coord;
        }

        return coords;
    }
}
