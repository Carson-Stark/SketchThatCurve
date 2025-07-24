using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu_unet : MonoBehaviour
{
    public GameObject[] panels;

    //for join panel
    public InputField nameField;
    public InputField codeField;
    public Button joinButton;
    public GameObject errortext;

    bool joinDisplayed;

    private Client network;

    // Start is called before the first frame update
    void Start()
    {
        joinDisplayed = false;
        errortext.SetActive(false);

        network = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<Client>();
        if (!network.isConnected)
        {
            displayPanel(0);
            network.Init(OnConnectedToMaster);
        }
        else
            displayPanel(1);
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

    public void OnConnectedToMaster()
    {
        Debug.Log("Connected");
        displayPanel(1);
    }

    public void CreateRoom()
    {
        string code = generateRoomCode();
        Debug.Log("attempting to create room " + code);
        network.CreateRoom(code, OnJoinedRoom);
    }

    public void joingameMainMenu()
    {
        displayPanel(2);
        joinDisplayed = true;
    }

    public void back()
    {
        displayPanel(1);
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
        string name = nameField.text;
        string code = codeField.text;

        network.JoinRoom(code, name, OnJoinedRoom);
    }

    public void OnJoinedRoom(bool success)
    {
        if (success)
        {
            Debug.Log("Successfully joined room " + network.currentRoom.roomName);
            SceneManager.LoadScene("GameLobby");
        }
        else
        {
            Debug.Log("Room join failed");

            if (joinDisplayed)
            {
                //if (returnCode == 32758)
                errortext.SetActive(true);
                //else
                //    errortext.SetActive(false);
            }
        }
    }

    /*public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Connection Error: " + cause);
    }*/
}
