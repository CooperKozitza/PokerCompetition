using Poker;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    public GameObject cardPrefab;
    public List<GameObject> cards;

    public Poker.Player player;

    public TextMeshProUGUI bidText;
    public TextMeshProUGUI nameText;

    public GameObject chips;
    public GameObject dealer;
    public GameObject highlight;
    public GameObject fold;

    public void SetPlayer(Poker.Player player)
    {
        this.player = player;
    }

    void Start()
    {
        for (int i = 0; i < 2; ++i)
        {
            cards.Add(this.transform.GetChild(i).gameObject);
        }

        chips = this.transform.GetChild(2).gameObject;

        nameText = this.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>();
        bidText = this.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>();

        fold = this.transform.GetChild(5).gameObject;
        highlight = this.transform.GetChild(6).gameObject;
        dealer = this.transform.GetChild(7).gameObject;

        nameText.text = "Loading...";
        bidText.text = "0";

        fold.SetActive(false);
        highlight.SetActive(false);
        dealer.SetActive(false);
    }

    private bool changingBid = false;
    private IEnumerator SetBid(float bid)
    {
        changingBid = true;

        float delta_bid = bid - float.Parse(bidText.text);
        float step = delta_bid / 100;

        while (Mathf.Abs(bid - float.Parse(bidText.text)) < 0.1)
        {
            bidText.text = (float.Parse(bidText.text) + step).ToString();
            yield return null;
        }

        bidText.text = bid.ToString();

        changingBid = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) { return; }

        nameText.text = player.Name;

        for (int i = 0; i < 2; i++)
        {
            cards[i].GetComponent<Image>().sprite = player.Hand[i].GetSprite();
        }

        highlight.SetActive(player.MyTurn);
        dealer.SetActive(player.Dealer);
        fold.SetActive(player.Folded);
        chips.SetActive(!player.Folded);
        bidText.gameObject.SetActive(!player.Folded);

        if (player.CurrentBet != float.Parse(bidText.text) && !changingBid)
        {
            StartCoroutine(SetBid(player.CurrentBet));
        }
    }
}
