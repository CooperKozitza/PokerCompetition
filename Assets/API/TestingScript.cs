using Poker;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;

public class TestingScript : MonoBehaviour
{
    [SerializeField]
    public GameManager.GameState gameState;

    public GameManager gameManager;
    public TestPlayer1 player1;
    public TestPlayer2 player2;
    public TestPlayer3 player3;

    public TextMeshProUGUI textMeshPro;
    public Canvas canvas;

    public GameObject playerUITemplate;

    public float RADIUS_COEF;
    public float CARD_SPACING_COEF;

    private Dictionary<Card, Sprite> cardImageCache = new Dictionary<Card, Sprite>();
    private List<GameObject> cards = new List<GameObject>();

    private void Start()
    {
        gameManager = GameManager.Instance;
        player1 = new TestPlayer1();
        player2 = new TestPlayer2();
        player3 = new TestPlayer3();

        gameManager.Deal();

        gameManager.UpdateState();
        gameState = gameManager.GetState();
        int playerCount = 0;
        foreach (Player player in gameState.Players)
        {
            float angle = (2.0f * (float)Math.PI) * ((float)playerCount / gameState.Players.Count);

            Vector2 playerBasePosition;
            playerBasePosition.x = (float)Math.Cos(angle) * RADIUS_COEF * (canvas.pixelRect.width / 2);
            playerBasePosition.y = (float)Math.Sin(angle) * RADIUS_COEF * (canvas.pixelRect.height / 2);

            GameObject playerUI = Instantiate(playerUITemplate, canvas.transform);
            playerUI.GetComponent<RectTransform>().localPosition = playerBasePosition;
            playerUI.GetComponent<PlayerScript>().SetPlayer(player);

            playerCount++;
        }

        StartCoroutine(ExecuteTurns());
    }

    private bool isDisplayingMessages = false;

    private IEnumerator DisplayMessages()
    {
        isDisplayingMessages = true;

        while (gameManager.GetState().MessageCount > 0)
        {
            string message = gameManager.GetMessage().ToString();
            
            foreach (char c in message)
            {
                textMeshPro.text += c;
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(3.0f);

            textMeshPro.text = string.Empty;
        }

        isDisplayingMessages = false;

    }

    private IEnumerator ExecuteTurns()
    {
        Player[] players = { player1, player2, player3 };
        foreach(Player p in players)
        {
            p.MyTurn = true;
            p.OnTurn(gameManager.GetState());

            while (gameManager.GetState().MessageCount > 0)
            {
                yield return null;
            }

            gameManager.ExecuteActions();

            p.MyTurn = false;
        }
    }

    private void Update()
    {
        gameState = gameManager.GetState();
        if (gameState.MessageCount > 0 && !isDisplayingMessages)
        {
            StartCoroutine(DisplayMessages());
        }
    }
}