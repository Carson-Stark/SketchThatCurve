using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject[] panels;
    public Graphing playerGraph;
    public DisplayGraph playerScoreGraph;
    public DisplayGraph bestGraph;
    public DisplayGraph hostSolution;
    public Leaderboard leaderboard;
    public Leaderboard finalLeaderboard;

    private int questionsInGame;
    public float slopeScoreWeight;
    public float percentTreshold;
    public float maxPoints;
    public float maxTimePenalty;
    public float roundStrtCountdown;

    private Text func_txt;
    private Text time_txt;
    private Text submitted_time_txt;

    private Text bestPlayer_txt;
    private Text bdscore_txt;
    private Text bsscore_txt;
    private Text timespent_txt;

    private Text countdown_txt;
    private Text getready_txt;

    //host only
    private Text func_num_txt;
    private Text submitted_txt;
    public GameObject startNewRoundBut;

    //player only
    public Button submitGraphBut;
    private Text dscore_txt;
    private Text sscore_txt;
    private Text scoreChange_txt;
    private Text correct_txt;
    private Text score_change_txt;

    QuestionManager questionManager;
    Question currentQ;
    int questionCount = 0;
    int easyCount;
    int mediumCount;
    int hardCount;
    bool roundStarted;
    bool submitted;
    bool waitingNextRound;
    bool countingDownNextRound;
    float timeRemaining;
    float countdown;

    bool gameFinished;

    int myTotalScore;
    GraphSubmission mysubmission;

    //host only
    Dictionary<Player, int> scores;
    Dictionary<Player, GraphSubmission> submittedGraphs;
    Player bestGraphPlayer;
    float bestGraphScore;
    bool solutionHidden;

    Player masterClient;

    // Start is called before the first frame update
    void Start()
    {
        masterClient = PhotonNetwork.MasterClient;

        questionManager = GetComponent<QuestionManager>();
        scores = new Dictionary<Player, int>();
        submittedGraphs = new Dictionary<Player, GraphSubmission>();

        easyCount = GameSettings.easyAmount;
        mediumCount = GameSettings.mediumAmount;
        hardCount = GameSettings.hardAmount;

        questionsInGame = easyCount + mediumCount + hardCount;

        displayPanel(6);
        countdown_txt = panels[6].transform.Find("countdown").GetComponent<Text>();
        getready_txt = panels[6].transform.Find("ready").GetComponent<Text>();
        func_num_txt = GameObject.Find("num").GetComponent<Text>();

        if (PhotonNetwork.IsMasterClient)
        {
            func_txt = panels[0].transform.Find("Function").GetComponent<Text>();
            time_txt = panels[0].transform.Find("Timeleft").GetComponent<Text>();
            submitted_txt = panels[0].transform.Find("Recieved Count").GetComponent<Text>();

            nextRound();
        }
        else
        {
            func_txt = panels[1].transform.Find("Function").GetComponent<Text>();
            time_txt = panels[1].transform.Find("Timeleft").GetComponent<Text>();
            submitted_time_txt = panels[2].transform.Find("Timeleft").GetComponent<Text>();

            dscore_txt = panels[3].transform.Find("direct score").GetComponent<Text>();
            sscore_txt = panels[3].transform.Find("slope score").GetComponent<Text>();
            correct_txt = panels[3].transform.Find("correct").GetComponent<Text>();
            scoreChange_txt = panels[3].transform.Find("score change").GetComponent<Text>();
        }

        bdscore_txt = panels[4].transform.Find("direct score").GetComponent<Text>();
        bsscore_txt = panels[4].transform.Find("slope score").GetComponent<Text>();
        bestPlayer_txt = panels[4].transform.Find("player").GetComponent<Text>();
        timespent_txt = panels[4].transform.Find("timespent").GetComponent<Text>();
    }

    private void displayPanel(int panel)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == panel)
                panels[i].SetActive(true);
            else
                panels[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (countingDownNextRound)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0)
            {
                roundStarted = true;
                countingDownNextRound = false;
                if (PhotonNetwork.IsMasterClient)
                {
                    displayPanel(0);
                    this.photonView.RPC("hostReadyForSubs", RpcTarget.Others);
                }
                else
                    displayPanel(1);
            }
            countdown_txt.text = "" + Mathf.RoundToInt(countdown);
        }

        if (roundStarted)
        {
            timeRemaining = Mathf.Min(currentQ.time * 60, timeRemaining - Time.deltaTime);
            if (!submitted)
                time_txt.text = "Time Remaining: " + timeToString(timeRemaining);
            else
                submitted_time_txt.text = "Time Remaining: " + timeToString(timeRemaining);

            if (timeRemaining <= 0)
            {
                roundStarted = false;
                if (!PhotonNetwork.IsMasterClient && !submitted)
                    submitGraph();
                else
                    time_txt.text = "Time Left: 0:00";
            }
        }
    }

    string timeToString(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.CeilToInt(time % 60);
        return minutes + ":" + (seconds < 10 ? "0" : "") + seconds;
    }

    [PunRPC]
    void hostReadyForSubs()
    {
        submitGraphBut.interactable = true;
    }

    //host only
    public void nextRound()
    {
        int num = 0;
        int diff = 0;
        float t = 0;

        if (easyCount > 0)
        {
            diff = 0;
            num = Random.Range(0, questionManager.easyFunctions.Count);
            t = GameSettings.easyTime;
            easyCount--;
        }
        else if (mediumCount > 0)
        {
            diff = 1;
            num = Random.Range(0, questionManager.mediumFunctions.Count);
            t = GameSettings.mediumTime;
            mediumCount--;
        }
        else if (hardCount > 0)
        {
            diff = 2;
            num = Random.Range(0, questionManager.hardFunctions.Count);
            t = GameSettings.hardTime;
            hardCount--;
        }
        else
        {
            this.photonView.RPC("showFinalLeaderboard", RpcTarget.All);
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            Debug.LogFormat("Question {0} {1} broadcasted", num, diff);
            this.photonView.RPC("startRound", RpcTarget.All, num, diff, t);
        }
        else
            this.photonView.RPC("showFinalLeaderboard", RpcTarget.All);
    }

    [PunRPC]
    void startRound(int number, int difficulty, float t)
    {
        Debug.LogFormat("Question {0} {1} started", number, difficulty);
        questionCount++;

        mysubmission = null;
        submittedGraphs.Clear();
        bestGraphPlayer = null;
        bestGraphScore = -1;
        solutionHidden = true;

        hostSolution.resetGraph();
        playerGraph.resetGraph();
        playerScoreGraph.resetGraph();
        bestGraph.resetGraph();

        currentQ = questionManager.GetQuestion(number, difficulty);
        currentQ.time = t;
        timeRemaining = currentQ.time * 60 + 2;
        submitted = false;
        waitingNextRound = false;

        foreach (GameObject p in panels)
            p.SetActive(true);
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Function"))
            go.GetComponent<Text>().text = "f(x) = " + currentQ.text;

        countingDownNextRound = true;
        countdown = roundStrtCountdown;

        displayPanel(6);
        if (PhotonNetwork.IsMasterClient)
        {
            getready_txt.text = "";
            this.photonView.RPC("updateFuncNum", RpcTarget.All, questionCount, questionsInGame);
            submitted_txt.text = string.Format("0/{0} Graphs Submitted", PhotonNetwork.CurrentRoom.PlayerCount - 1);
            IEnumerator drawSolution = hostSolution.drawGraph((x) => currentQ.gety(x), currentQ.sizex, currentQ.sizey, 250, Color.blue, true);
            StartCoroutine(drawSolution);
        }
        else
        {
            playerGraph.initializeBounds(currentQ.str_sizex, currentQ.str_sizey);
            submitGraphBut.interactable = false;
        }
    }

    [PunRPC]
    void updateFuncNum(int current, int total)
    {
        func_num_txt.text = string.Format("Function {0}/{1}", current, total);
    }

    //player only
    public void submitGraph()
    {
        submitGraphBut.GetComponentInChildren<Text>().text = "Submitting...";

        mysubmission = new GraphSubmission();
        Point[] points = playerGraph.getGraphCoords(currentQ.sizex, currentQ.sizey);
        mysubmission.graph = points;
        mysubmission.directScore = scoreDirect(points, currentQ, 10);
        mysubmission.slopeScore = scoreSlope(points, currentQ, 4);
        mysubmission.timeSpent = Mathf.RoundToInt(currentQ.time * 60 - timeRemaining);
        float totalScore = mysubmission.slopeScore * slopeScoreWeight + mysubmission.directScore * (1 - slopeScoreWeight);
        mysubmission.totalScore = totalScore;
        int scoreChange = 0;
        if (mysubmission.directScore > percentTreshold && mysubmission.slopeScore > percentTreshold)
        {
            float accuracyPoints = maxPoints * ((totalScore - percentTreshold) / (100 - percentTreshold));
            float timePenalty = maxTimePenalty * ((currentQ.time * 60 - timeRemaining) / (currentQ.time * 60));
            Debug.Log("A: " + accuracyPoints);
            Debug.Log("T: " + timePenalty);
            scoreChange = Mathf.Max(1, Mathf.RoundToInt(accuracyPoints - timePenalty));
        }
        mysubmission.scoreChange = scoreChange;
        myTotalScore += scoreChange;

        this.photonView.RPC("recievePlayerGraph", RpcTarget.MasterClient, serializeGraph(mysubmission));
        displayPanel(2);
        playerGraph.resetGraph();
        submitted = true;

        submitGraphBut.GetComponentInChildren<Text>().text = "Submit";
    }

    [PunRPC]
    //only for host
    public void recievePlayerGraph(byte[] bytegraph, PhotonMessageInfo info)
    {
        GraphSubmission recgraph = deserializeGraph(bytegraph);

        Debug.Log("Recieved graph from " + info.Sender.NickName);

        if (!submittedGraphs.ContainsKey(info.Sender))
        {
            submittedGraphs.Add(info.Sender, recgraph);
            if (scores.ContainsKey(info.Sender))
                scores[info.Sender] += recgraph.scoreChange;
            else
                scores.Add(info.Sender, recgraph.scoreChange);
        }
        submitted_txt.text = string.Format("{0}/{1} Graphs Submitted", submittedGraphs.Count, PhotonNetwork.CurrentRoom.PlayerCount - 1);

        if (recgraph.totalScore > bestGraphScore)
        {
            bestGraphScore = recgraph.totalScore;
            bestGraphPlayer = info.Sender;
        }

        if (submittedGraphs.Count >= PhotonNetwork.CurrentRoom.PlayerCount - 1)
            this.photonView.RPC("endRound", RpcTarget.All, bestGraphPlayer.NickName, serializeGraph(submittedGraphs[bestGraphPlayer]));
    }

    public void endRoundButton()
    {
        this.photonView.RPC("forceSubmissions", RpcTarget.Others);
    }

    [PunRPC]
    public void endRound(string bestPlayerName, byte[] bestSubBytes)
    {
        Debug.Log("ending round");
        if (waitingNextRound)
            return;

        GraphSubmission bestSub = deserializeGraph(bestSubBytes);

        if (!PhotonNetwork.IsMasterClient)
        {
            //set up score planel
            displayPanel(3);
            IEnumerator drawactual = playerScoreGraph.drawGraph((x) => currentQ.gety(x), currentQ.sizex, currentQ.sizey, 250, new Color(0, 0, 1, 0.5f));
            StartCoroutine(drawactual);
            IEnumerator drawPlayer = playerScoreGraph.drawGraph(mysubmission.graph, currentQ.sizex, currentQ.sizey, Color.black);
            StartCoroutine(drawPlayer);
            if (mysubmission.scoreChange > 0)
            {
                correct_txt.text = "Correct!";
                correct_txt.color = Color.blue;
            }
            else
            {
                correct_txt.text = "Oops... Maybe Next Time";
                correct_txt.color = Color.red;
            }
            dscore_txt.text = "Direct Comparison: " + Mathf.Round(mysubmission.directScore) + "%";
            sscore_txt.text = "Slope Comparison: " + Mathf.Round(mysubmission.slopeScore) + "%";
            scoreChange_txt.text = (myTotalScore - mysubmission.scoreChange) + "  + " + mysubmission.scoreChange;
            waitingNextRound = true;
        }
        else
        {
            displayPanel(4);
            generateLeaderboard();
        }

        //set up best graph
        bestPlayer_txt.text = bestPlayerName + " had the best graph!";
        bdscore_txt.text = "Direct Comparison: " + Mathf.Round(bestSub.directScore) + "%";
        bsscore_txt.text = "Slope Comparison: " + Mathf.Round(bestSub.slopeScore) + "%";
        timespent_txt.text = "Time Spent: " + timeToString(bestSub.timeSpent);
        IEnumerator drawActual = bestGraph.drawGraph((x) => currentQ.gety(x), currentQ.sizex, currentQ.sizey, 250, new Color(0, 0, 1, 0.5f));
        StartCoroutine(drawActual);
        IEnumerator drawBest = bestGraph.drawGraph(bestSub.graph, currentQ.sizex, currentQ.sizey, Color.black);
        StartCoroutine(drawBest);

        roundStarted = false;
        submitted = true;
        waitingNextRound = true;
        timeRemaining = 0;
        time_txt.text = "Time Remaining: 0:00";
    }

    [PunRPC]
    void forceSubmissions()
    {
        if (!submitted)
        {
            submitGraph();
            displayPanel(2);
        }
    }

    [PunRPC]
    void showFinalLeaderboard()
    {
        displayPanel(7);
        gameFinished = true;
    }

    void generateLeaderboard()
    {
        List<Player> players2Add = new List<Player>(scores.Keys);
        string[] sortedPlayers = new string[players2Add.Count];
        int[] sortedScores = new int[players2Add.Count];
        for (int i = 0; i < scores.Keys.Count; i++)
        {
            int maxScore = 0;
            Player nextBest = players2Add[0];
            foreach (Player p in players2Add)
            {
                if (scores[p] > maxScore)
                {
                    maxScore = scores[p];
                    nextBest = p;
                }
            }
            players2Add.Remove(nextBest);
            sortedPlayers[i] = nextBest.NickName;
            sortedScores[i] = maxScore;
        }

        this.photonView.RPC("updateLeaderboard", RpcTarget.All, sortedPlayers, sortedScores);
    }

    [PunRPC]
    void updateLeaderboard(string[] names, int[] playerScores)
    {
        int localPlayer = -1;
        if (!PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == PhotonNetwork.LocalPlayer.NickName && playerScores[i] == myTotalScore)
                    localPlayer = i;
            }
        }

        leaderboard.updateBoard(names, playerScores, localPlayer);
        finalLeaderboard.updateBoard(names, playerScores, localPlayer);
        finalLeaderboard.changeTitle(names[0] + " Wins!");
    }

    [PunRPC]
    public void quitGame(bool isFinal = false)
    {
        if (PhotonNetwork.IsMasterClient && !isFinal)
        {
            roundStarted = false;
            if (questionCount > 1)
            {
                generateLeaderboard();
                this.photonView.RPC("showFinalLeaderboard", RpcTarget.All);
            }
            else
            {
                this.photonView.RPC("quitGame", RpcTarget.Others, true);
                isFinal = true;
            }
        }

        if (isFinal || !PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Main Menu");
        }
    }

    byte[] serializeGraph(GraphSubmission graph)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        formatter.Serialize(ms, graph);
        return ms.ToArray();
    }

    GraphSubmission deserializeGraph(byte[] graph)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(graph);
        return (GraphSubmission)formatter.Deserialize(ms);
    }

    public void changeSolutionVisibility(Text txt)
    {
        if (solutionHidden)
        {
            hostSolution.revealGraph();
            txt.text = "Hide Solution";
        }
        else
        {
            hostSolution.hideGraph();
            txt.text = "Show Solution";
        }
        solutionHidden = !solutionHidden;
    }

    public void continueFromScore()
    {
        displayPanel(4);
    }

    public void continueFromBest()
    {
        if (!PhotonNetwork.IsMasterClient)
            startNewRoundBut.SetActive(false);
        else if (questionCount >= questionsInGame || PhotonNetwork.CurrentRoom.PlayerCount == 1)
            startNewRoundBut.GetComponentInChildren<Text>().text = "Finish";

        displayPanel(5);
    }

    float scoreDirect(Point[] points, Question question, int steps)
    {
        float checkRadius = question.sizex / 4;
        float totalDifference = 0;
        int pointsUsed = 0;
        for (int p = 0; p < points.Length; p++)
        {
            if (points[p].y == Mathf.Infinity && !isValid(question.gety(points[p].x), question.sizey))
                continue;
            else if (!isReal(question.gety(points[p].x)))
            {
                totalDifference += question.sizey * 2;
                pointsUsed++;
            }
            else
            {
                float stepSize = (checkRadius * 2) / steps;
                float minDistance = question.sizey * 2;
                for (float x = points[p].x - checkRadius; x < points[p].x + checkRadius; x += stepSize)
                {
                    float distance = Vector2.Distance(new Vector2(points[p].x, points[p].y), new Vector2(x, question.gety(x)));
                    if (distance < minDistance)
                        minDistance = distance;
                }
                Debug.Log(minDistance);
                totalDifference += minDistance;
                pointsUsed++;
            }
        }
        if (pointsUsed == 0)
            return 0;
        float directScore = 100 - ((totalDifference / pointsUsed) / (question.sizey * 1.5f) * 100); //percent average diff / max diff and then reversed
        return Mathf.Max(0, directScore);
    }

    /*float scoreDirect(Point[] points, Question question)
    {
        float totalDifference = 0;
        int pointsUsed = 0;
        foreach (Point point in points)
        {
            float actualy = question.gety(point.x);
            if (point.y < Mathf.Infinity && isReal(actualy))
            {
                float realDiffFromBounds = Mathf.Min(question.sizey / 2, Mathf.Abs(actualy) - question.sizey);
                Debug.Log(realDiffFromBounds);
                if (actualy > question.sizey)
                    totalDifference += Mathf.Abs(question.sizey + realDiffFromBounds - point.y);
                else if (actualy < -question.sizey)
                    totalDifference += Mathf.Abs(-question.sizey - realDiffFromBounds - point.y);
                else
                    totalDifference += Mathf.Abs(actualy - point.y);
                pointsUsed++;
            }
            else if ((isValid(actualy, question.sizey) && point.y == Mathf.Infinity) || (actualy < Mathf.Infinity && !isReal(actualy)))
            {
                totalDifference += question.sizey * 2;
                pointsUsed++;
            }
        }
        if (pointsUsed == 0)
            return 0;
        float directScore = 100 - ((totalDifference / pointsUsed) / (question.sizey * 1.5f) * 100); //percent average diff / max diff and then reversed
        return Mathf.Max(0, directScore);
    }*/

    float scoreSlope(Point[] points, Question question, int coordLength)
    {
        float totalDifference = 0;
        float xChange = 0;
        int pointsUsed = 0;
        for (int p = 0; p < points.Length - 4; p++)
        {
            /*xChange = points[p + 4].x - points[p - 4].x;
            float playerSlope = (points[p + 4].y - points[p - 4].y) / (xChange);
            float actualSlope = (question.gety(points[p + 4].x) - question.gety(points[p - 4].x)) / (xChange);*/
            float yoffset = question.gety(points[p].x) - points[p].y;
            float diff = (Mathf.Atan2(question.gety(points[p + coordLength].x) - yoffset - points[p].y, points[p + coordLength].x - points[p].x) -
                Mathf.Atan2(points[p + coordLength].y - points[p].y, points[p + coordLength].x - points[p].x)) * (180 / Mathf.PI);

            if (isReal(diff))
            {
                totalDifference += Mathf.Abs(diff);
                pointsUsed++;
            }
        }
        if (pointsUsed == 0)
        {
            totalDifference = 90;
            pointsUsed = 1;
        }
        //xChange = (float)question.sizex * 2 / points.Length;
        float slopeScore = Mathf.Max(0, 100 - ((totalDifference / pointsUsed) / 90 * 100)); //percent average diff / max diff and then reversed
        return slopeScore;
    }

    bool isReal(float y)
    {
        return !float.IsNaN(y) && y != Mathf.Infinity && y != Mathf.NegativeInfinity;
    }

    bool isValid(float y, float sizey)
    {
        return Mathf.Abs(y) < sizey && isReal(y);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer == masterClient && PhotonNetwork.IsMasterClient && !gameFinished)
        {
            quitGame();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Connection Error: " + cause);
        SceneManager.LoadScene(0);
    }
}
