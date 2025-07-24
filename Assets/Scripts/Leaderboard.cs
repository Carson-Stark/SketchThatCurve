using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    public GameObject listing;
    public int maxPlayersToShow;

    List<GameObject> listings;

    void Awake()
    {
        listings = new List<GameObject>();
    }

    public void changeTitle(string title)
    {
        GetComponent<Text>().text = title;
    }

    public void updateBoard(string[] players, int[] scores, int localPlayer)
    {
        clearListings();

        for (int i = 0; i < maxPlayersToShow && i < players.Length; i++)
        {
            GameObject l = (GameObject)Instantiate(listing, transform);
            l.GetComponent<Text>().text = string.Format("{0}. {1} ({2})", i + 1, players[i], scores[i]);
            listings.Add(l);
        }

        if (localPlayer >= maxPlayersToShow)
        {
            GameObject l = (GameObject)Instantiate(listing, transform);
            l.GetComponent<Text>().text = string.Format("{0}. {1} ({2})", localPlayer + 1, players[localPlayer], scores[localPlayer]);
            listings.Add(l);
        }
    }

    void clearListings()
    {
        List<GameObject> listingsCone = new List<GameObject>(listings);
        foreach (GameObject l in listingsCone)
        {
            Destroy(l);
            listings.Remove(l);
        }
    }
}
