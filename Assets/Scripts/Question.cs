using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Question
{
    public Sprite image;
    public string text;
    public float time;

    public float sizex = 10;
    public float sizey = 10;
    public string str_sizex = "10";
    public string str_sizey = "10";

    public delegate float function(float x);
    public function gety;

    public Question(string txt, Sprite img, function f, float sx = 10, float sy = 10, string str_sx = "10", string str_sy = "10")
    {
        image = img;
        text = txt;
        gety = f;

        sizex = sx;
        sizey = sy;

        str_sizex = str_sx;
        str_sizey = str_sy;
    }
}