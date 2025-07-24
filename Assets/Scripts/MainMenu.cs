using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class MainMenu : MonoBehaviourPunCallbacks
{
    public GameObject menuPanel;
    public GameObject[] panels;
    public GameObject instuPanel;
    public GameObject tipsPanel;

    public Text connecting_txt;
    public GameObject retryButton;

    //for join panel
    public InputField nameField;
    public InputField codeField;
    public Button createButton;
    public Button joinButton;
    public GameObject errortext;

    bool joinDisplayed;

    // Start is called before the first frame update
    void Start()
    {
        joinDisplayed = false;
        errortext.SetActive(false);
        retryButton.SetActive(false);
        instuPanel.SetActive(false);
        tipsPanel.SetActive(false);

        if (!PhotonNetwork.IsConnected)
        {
            displayPanel(0);
            connect();
        }
        else
            displayPanel(1);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.R) && PhotonNetwork.IsConnectedAndReady)
        {
            RoomOptions opt = new RoomOptions();
            PhotonNetwork.JoinOrCreateRoom("Dev room", opt, new TypedLobby("lob", LobbyType.Default));
        }
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

    public void connect()
    {
        connecting_txt.text = "Connecting...";
        connecting_txt.color = Color.black;
        retryButton.SetActive(false);
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = "1";
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected");
        displayPanel(1);
    }

    public void showInstructions()
    {
        menuPanel.SetActive(false);
        instuPanel.SetActive(true);
    }

    public void showTips()
    {
        menuPanel.SetActive(false);
        tipsPanel.SetActive(true);
    }

    public void backFromPanel()
    {
        menuPanel.SetActive(true);
        instuPanel.SetActive(false);
        tipsPanel.SetActive(false);
    }

    public void CreateRoom()
    {
        createButton.interactable = false;
        createButton.GetComponentInChildren<Text>().text = "Creating...";
        string code = generateRoomCode();
        Debug.Log("creating room " + code);
        PhotonNetwork.CreateRoom(code);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (returnCode == 32766)
        {
            Debug.Log("room already exists");
            CreateRoom();
        }
    }

    public void joingameMainMenu()
    {
        displayPanel(2);
        joinDisplayed = true;
    }

    public void back()
    {
        displayPanel(1);
        joinDisplayed = false;
    }

    public void checkRoomInputs()
    {
        if (!joinButton.interactable && nameField.text.Length > 0 && codeField.text.Length == 5)
        {
            joinButton.interactable = true;
        }
        else if (nameField.text.Length == 0 || codeField.text.Length < 5)
        {
            joinButton.interactable = false;
        }
    }

    private string generateRoomCode()
    {
        string code = "";
        for (int i = 0; i < 5; i++)
            code += Random.Range(0, 10).ToString();
        return code;
    }

    public void joinGame()
    {
        joinButton.interactable = false;
        joinButton.GetComponentInChildren<Text>().text = "Joining...";

        string name = nameField.text;
        string code = codeField.text;

        PhotonNetwork.LocalPlayer.NickName = name;
        PhotonNetwork.JoinRoom(code);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogFormat("Room join failed with error code {0} and error message {1}", returnCode, message);
        joinButton.interactable = true;
        joinButton.GetComponentInChildren<Text>().text = "Join Game";

        if (joinDisplayed)
        {
            errortext.SetActive(true);
            Text error_txt = errortext.GetComponent<Text>();
            switch (returnCode)
            {
                case 32758:
                    error_txt.text = "Invalid Game Code";
                    break;
                case 32764:
                    error_txt.text = "Game Already Started";
                    break;
                case 32765:
                    error_txt.text = "Game Is Full";
                    break;
                default:
                    error_txt.text = "Unknown Error";
                    break;
            }
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined room " + PhotonNetwork.CurrentRoom.Name);
        SceneManager.LoadScene("GameLobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Connection Error: " + cause);
        displayPanel(0);
        connecting_txt.color = Color.red;
        retryButton.SetActive(true);

        if (cause == DisconnectCause.MaxCcuReached)
            connecting_txt.text = "Server is full. Try again later";
        else
        {
            connecting_txt.text = "Unable to connect. Check Internet";
        }
    }
}
