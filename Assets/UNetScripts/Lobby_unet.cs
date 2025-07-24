using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lobby_unet : MonoBehaviour
{
    public Text gameCodeTxt;
    public GameObject playerListing;
    public GameObject playerPanel;
    public GameObject hostUI;
    public GameObject playerUI;

    private List<GameObject> playerNameList;

    private Client network;

    // Start is called before the first frame update
    void Start()
    {
        network = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<Client>();
        network.RoomStatusChangeCallback = OnRoomUpdate;

        gameCodeTxt.text = "Game Code: " + network.currentRoom.roomName;
        playerNameList = new List<GameObject>();

        if (network.isRoomHost)
        {
            playerUI.SetActive(false);
            hostUI.SetActive(true);
        }
        else
        {
            hostUI.SetActive(false);
            playerUI.SetActive(true);

            foreach (NetworkPlayer player in network.currentRoom.players)
            {
                addPlayerListing(player);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void addPlayerListing(NetworkPlayer player)
    {
        GameObject listing = (GameObject)Instantiate(playerListing, playerPanel.transform);
        listing.GetComponent<Text>().text = player.nickname;
        playerNameList.Add(listing);
        playerPanel.GetComponent<Text>().text = playerNameList.Count != 1 ? playerNameList.Count.ToString() + " Players" : "1 Player";
    }

    public void leaveGame()
    {
        network.LeaveRoom();
        SceneManager.LoadScene("Main Menu");
    }

    public void startGame()
    {

    }

    public void OnRoomUpdate(NetworkPlayer otherPlayer, bool entered)
    {
        if (entered)
            addPlayerListing(otherPlayer);
        else
        {
            foreach (GameObject listing in playerNameList)
            {
                if (listing.GetComponent<Text>().text == otherPlayer.nickname)
                    Destroy(listing);
            }
        }

    }
}

