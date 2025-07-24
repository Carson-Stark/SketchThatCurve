using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    public Sprite[] easyFSprites;
    public Sprite[] mediFSprites;
    public Sprite[] hardFSprites;

    public List<Question> easyFunctions;
    public List<Question> mediumFunctions;
    public List<Question> hardFunctions;


    // Start is called before the first frame update
    void Start()
    {
        Sprite dummySprite = easyFSprites[0];

        easyFunctions = new List<Question>();
        easyFunctions.Add(new Question("2x+3", easyFSprites[0], (x) => Functions.Polynomial(x, 3, 2)));
        easyFunctions.Add(new Question("x^2 + 1", easyFSprites[1], (x) => Functions.Polynomial(x, 1, 0, 2)));
        easyFunctions.Add(new Question("(x + 3)^3", easyFSprites[2], (x) => Mathf.Pow(x + 3, 3)));
        easyFunctions.Add(new Question("x^2 + x + 1", easyFSprites[3], (x) => Functions.Polynomial(x, 1, 1, 2)));
        easyFunctions.Add(new Question("-x^3 + (1/2)x^2 + 3x + 3", easyFSprites[4], (x) => Functions.Polynomial(x, 3, 3, 0.5f, -1)));

        easyFunctions.Add(new Question("asin(x)", dummySprite, (x) => Mathf.Asin(x), 1, Mathf.PI, "1", "pi"));
        easyFunctions.Add(new Question("acot(x)", dummySprite, (x) => -Mathf.Atan(x) + Mathf.PI / 2));
        easyFunctions.Add(new Question("(cos(x) + 1)^(1/2)", dummySprite, (x) => Mathf.Sqrt((1 + Mathf.Cos(x)) / 2), 2 * Mathf.PI, 1, "2pi", "1"));
        easyFunctions.Add(new Question("x^2 + 1", dummySprite, (x) => Mathf.Pow(x, 2) + 1));
        easyFunctions.Add(new Question("tan(x)", dummySprite, (x) => Mathf.Tan(x), Mathf.PI, 10, "pi"));
        easyFunctions.Add(new Question("(-2/3)x + (5/3)", dummySprite, (x) => (-2.0f / 3.0f) * x + (5.0f / 3.0f)));
        easyFunctions.Add(new Question("sin(x)", dummySprite, (x) => Mathf.Sin(x), Mathf.PI, 1, "pi", "1"));

        hardFunctions = new List<Question>();
        hardFunctions.Add(new Question("atan(x^2)", dummySprite, (x) => Mathf.Atan(Mathf.Pow(x, 2))));
        hardFunctions.Add(new Question("x + tan(x)", dummySprite, (x) => x + Mathf.Tan(x), Mathf.PI, 10, "pi"));
        hardFunctions.Add(new Question("-12x^2 - 2x^3 + x^4", dummySprite, (x) => Functions.Polynomial(x, 0, 0, -12, -2, 1)));
        hardFunctions.Add(new Question("8 - 5x^4 + x^5", dummySprite, (x) => Functions.Polynomial(x, 8, 0, 0, 0, -5, 1)));
        hardFunctions.Add(new Question("5 - 8x^3 - x^4", dummySprite, (x) => Functions.Polynomial(x, 5, 0, 0, -8, -1)));
        hardFunctions.Add(new Question("x^(4/3) * (x-2)", dummySprite, (x) => Mathf.Pow(x, (4.0f / 3.0f)) * (x - 2)));
        hardFunctions.Add(new Question("1/(e^(-x) - 1)", dummySprite, (x) => 1 / (Mathf.Exp(-x) - 1)));
        hardFunctions.Add(new Question("x * (x^2 + 1)^(-1/2)", dummySprite, (x) => x / Mathf.Sqrt(Mathf.Pow(x, 2) + 1)));
        hardFunctions.Add(new Question("x - 3x^(2/3)", dummySprite, (x) => x - 3 * Mathf.Pow(x, (2.0f / 3.0f))));
        hardFunctions.Add(new Question("Ln(x)/x^2", dummySprite, (x) => Mathf.Log(x, Mathf.Exp(1)) / Mathf.Pow(x, 2)));
        hardFunctions.Add(new Question("2 - x^(2/3) + x^(4/3)", dummySprite, (x) => 2 - Mathf.Pow(x, (2.0f) / (3.0f)) + Mathf.Pow(x, (4.0f / 3.0f))));

        mediumFunctions = new List<Question>();
        mediumFunctions.Add(new Question("x*(x+3)^(1/2)", dummySprite, (x) => x * Mathf.Sqrt(x + 3)));
        mediumFunctions.Add(new Question("(2x-3)/(2x-8)", dummySprite, (x) => (2 * x - 3) / (2 * x - 8)));
        mediumFunctions.Add(new Question("e^(-x) * sin(x)", dummySprite, (x) => Mathf.Exp(-x) * Mathf.Sin(x), Mathf.PI, 10, "pi"));
        mediumFunctions.Add(new Question("e^((-x^2)/2)", dummySprite, (x) => Mathf.Exp(-Mathf.Pow(x, 2) / 2)));
        mediumFunctions.Add(new Question("1 + 2x + x^3", dummySprite, (x) => Functions.Polynomial(x, 1, 2, 0, 1)));
        mediumFunctions.Add(new Question("x^2 / (x-2)", dummySprite, (x) => Mathf.Pow(x, 2) / (x - 2)));
        mediumFunctions.Add(new Question("3 + 3x + (1/2)*x^2 - x^3", dummySprite, (x) => Functions.Polynomial(x, 3, 3, 0.5f, -1)));
        mediumFunctions.Add(new Question("Ln(x^2 + 1)", dummySprite, (x) => Mathf.Log(Mathf.Pow(x, 2) + 1, Mathf.Exp(1))));
        mediumFunctions.Add(new Question("3x/(x^2 - 1)", dummySprite, (x) => 3 * x / (Mathf.Pow(x, 2) - 1)));
        mediumFunctions.Add(new Question("x * e^(4x)", dummySprite, (x) => x * Mathf.Pow(4, x)));
        mediumFunctions.Add(new Question("sin^2(x/2 + pi/2)", dummySprite, (x) => Mathf.Pow(Mathf.Sin(x / 2 + Mathf.PI / 2), 2), Mathf.PI, 1, "pi", "1"));


    }

    // Update is called once per frame
    public Question GetQuestion(int number, int difficulty)
    {
        if (difficulty == 0)
        {
            Question q = easyFunctions[number];
            easyFunctions.Remove(q);
            return q;
        }
        else if (difficulty == 1)
        {
            Question q = mediumFunctions[number];
            mediumFunctions.Remove(q);
            return q;
        }
        else if (difficulty == 2)
        {
            Question q = hardFunctions[number];
            hardFunctions.Remove(q);
            return q;
        }
        else
            Debug.LogErrorFormat("Difficulty {0} is not valid", difficulty);

        return easyFunctions[0];
    }
}
