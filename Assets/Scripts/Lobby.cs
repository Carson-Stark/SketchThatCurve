using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviourPunCallbacks
{
    public Text gameCodeTxt;
    public GameObject playerListing;
    public GameObject playerPanel;
    public GameObject hostUI;
    public GameObject playerUI;
    public Button startGameBut;

    public InputField easyNum;
    public InputField mediumNum;
    public InputField hardNum;
    public InputField easyTime;
    public InputField mediumTime;
    public InputField hardTime;

    private List<GameObject> playerNameList;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        gameCodeTxt.text = "Game Code: " + PhotonNetwork.CurrentRoom.Name;
        playerNameList = new List<GameObject>();

        if (PhotonNetwork.IsMasterClient)
        {
            playerUI.SetActive(false);
            hostUI.SetActive(true);
            startGameBut.interactable = false;
        }
        else
        {
            hostUI.SetActive(false);
            playerUI.SetActive(true);

            List<Player> players = new List<Player>();
            players.AddRange(PhotonNetwork.CurrentRoom.Players.Values);
            foreach (Player player in players)
            {
                if (!player.IsMasterClient)
                    addPlayerListing(player);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void addPlayerListing(Player player)
    {
        GameObject listing = (GameObject)Instantiate(playerListing, playerPanel.transform);
        listing.GetComponent<Text>().text = player.NickName;
        playerNameList.Add(listing);
        playerPanel.GetComponent<Text>().text = playerNameList.Count != 1 ? playerNameList.Count.ToString() + " Players" : "1 Player";
    }

    [PunRPC]
    public void leaveGame()
    {
        if (PhotonNetwork.IsMasterClient)
            this.photonView.RPC("leaveGame", RpcTarget.Others);

        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Main Menu");
    }

    [PunRPC]
    public void startGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            return;

        startGameBut.interactable = false;
        startGameBut.GetComponentInChildren<Text>().text = "Starting...";

        PhotonNetwork.CurrentRoom.IsOpen = false;

        if (PhotonNetwork.IsMasterClient)
        {
            if (easyNum.text.Length > 0)
                GameSettings.easyAmount = int.Parse(easyNum.text);
            if (mediumNum.text.Length > 0)
                GameSettings.mediumAmount = int.Parse(mediumNum.text);
            if (hardNum.text.Length > 0)
                GameSettings.hardAmount = int.Parse(hardNum.text);
            if (easyTime.text.Length > 0)
                GameSettings.easyTime = float.Parse(easyTime.text);
            if (mediumTime.text.Length > 0)
                GameSettings.mediumTime = float.Parse(mediumTime.text);
            if (hardTime.text.Length > 0)
                GameSettings.hardTime = float.Parse(hardTime.text);
        }

        SceneManager.LoadScene("Game");
    }

    public override void OnPlayerEnteredRoom(Player otherPlayer)
    {
        addPlayerListing(otherPlayer);
        startGameBut.interactable = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        List<GameObject> nameListClone = new List<GameObject>(playerNameList);
        foreach (GameObject listing in nameListClone)
        {
            if (listing.GetComponent<Text>().text == otherPlayer.NickName)
            {
                playerNameList.Remove(listing);
                Destroy(listing);
            }
        }

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 1)
            startGameBut.interactable = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Connection Error: " + cause);
        SceneManager.LoadScene(0);
    }
}
