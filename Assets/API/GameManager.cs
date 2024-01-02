using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker;
using System;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using System.Threading;
using System.Runtime.CompilerServices;
using static Poker.GameManager;
using System.Linq;

namespace Poker
{
    public class GameManager
    {
        #region State

        /// <summary>
        /// All the players in the game
        /// </summary>
        private List<Player> RegisteredPlayers { get; set; }

        /// <summary>
        /// Adds a player to the current game
        /// </summary>
        /// <param name="player">The player to add</param>
        public void RegisterPlayer(Player player)
        {
            RegisteredPlayers.Add(player);
        }

        /// <summary>
        /// This contains all the necessarry information about the current
        /// state of the game. It is used by both the UI to display the game
        /// and by the players to make decisions.
        /// </summary>

        [Serializable]
        public class GameState
        {
            public enum Round { Blind, PreFlop, Flop, Turn, River, Showdown }
            public enum TurnState { Ready, Dealing, Executing, AwaitingPlayer }

            public GameState()
            {
                Players = new List<Player>();
                CommunityCards = new List<Card>();
            }

            /// <summary>
            /// The other players in the game. This can be used to figure out
            /// how much other players bet, if they have folded, and other
            /// relevant information.
            /// </summary>
            public List<Player> Players { get; set; }

            /// <summary>
            /// The players who are still in the round (ie. havn't folded)
            /// </summary>
            public List<Player> CurrentPlayers { get; set; }

            /// <summary>
            /// These are the cards in the middle of the table from the flop,
            /// pre-flop, and river.
            /// </summary>
            public List<Card> CommunityCards { get; private set; }

            /// <summary>
            /// The current player whos turn it is
            /// </summary>
            public Player CurrentPlayer { get; set; }

            /// <summary>
            /// The current round type
            /// </summary>
            public Round CurrentRoundType { get; set; }

            /// <summary>
            /// The current state of the turn, like if the game is waiting on a
            /// player to make an action, or the game to process the actions.
            /// </summary>
            public TurnState CurrentTurnState { get; set; }

            /// <summary>
            /// The player who is the dealer for the round
            /// </summary>
            public Player Dealer { get; set; }

            /// <summary>
            /// The current size of the pot
            /// </summary>
            public float PotSize { get; set; }

            /// <summary>
            /// The number of messages waiting to be displayed
            /// </summary>
            public int MessageCount { get; set; }
        }
        private GameState state;

        /// <summary>
        /// Updates the state at any time with information not specific to what
        /// round it is, or any of the players actions. Things like who is in
        /// the game, who hasn't folded, the current pot size, and the message
        /// display count are all updated.
        /// </summary>
        public void UpdateState()
        {
            if (state == null)
            {
                state = new GameState();
            }

            state.Players = RegisteredPlayers;
            state.CurrentPlayers = RegisteredPlayers.Where(p => !p.Folded).ToList();

            state.PotSize = state.CurrentPlayers.Sum(x => x.CurrentBet);

            state.MessageCount = MessageQueue.Count;
        }

        /// <summary>
        /// Sets the dealer to be <c>player</c>, removes the dealer flag from
        /// all other registered players
        /// </summary>
        /// <param name="player">The new dealer</param>
        private void SetDealer(Player player)
        {
            foreach (Player p in RegisteredPlayers)
            {
                p.Dealer = false;
            }

            state.Dealer = player;

            player.Dealer = true;
        }

        /// <summary>
        /// Sets the player whos turn it is to be <c>player</c> 
        /// </summary>
        /// <param name="player">The player whos turn it now is</param>
        private void SetCurrentPlayer(Player player)
        {
            foreach (Player p in RegisteredPlayers)
            {
                p.MyTurn = false;
            }

            state.CurrentPlayer = player;

            player.MyTurn = true;
        }

        /// <summary>
        /// Returns the current state of the game at any given moment
        /// </summary>
        /// <returns>The state of the game</returns>
        public GameState GetState() { return state; }

        #endregion

        #region Deck

        /// <summary>
        /// The current deck for the round. At the end of the round, the deck
        /// is cleared, rebuilt, and shuffled before being dealt.
        /// </summary>
        public Stack<Card> Deck { get; set; }

        private void ResetDeck()
        {
            List<Card> tempDeck = new List<Card>();

            // Add all the cards
            foreach (Card.Suits suit in Enum.GetValues(typeof(Card.Suits)))
            {
                foreach (Card.Ranks rank in Enum.GetValues(typeof(Card.Ranks)))
                {
                    tempDeck.Add(new Card(suit, rank));
                }
            }

            // Shuffle using Fisher-Yates algorithm
            System.Random randomNumberGenerator = new System.Random();
            int count = tempDeck.Count;
            while (count > 1)
            {
                count--;

                int k = randomNumberGenerator.Next(count + 1);

                // Swap
                Card value = tempDeck[k];
                tempDeck[k] = tempDeck[count];
                tempDeck[count] = value;
            }

            // Clear the existing deck and push shuffled cards
            Deck.Clear();
            foreach (var card in tempDeck)
            {
                Deck.Push(card);
            }
        }
        public void Deal()
        {
            state.CurrentTurnState = GameState.TurnState.Dealing;

            ResetDeck();

            foreach (Player player in state.Players)
            {
                player.Hand = new List<Card>();

                if (Deck.Count < 2)
                {
                    throw new Exception("Deck ran out of cards");
                }

                for (short i = 0; i < 2; ++i)
                {
                    player.Hand.Add(Deck.Pop());
                }
            }

            state.CurrentTurnState = GameState.TurnState.Ready;
        }

        #endregion

        #region Actions and Messages

        /// <summary>
        /// This is the type of action an action is. When sending an action to
        /// the game manager from a player, the <c>ActionType</c> must be
        /// specified
        /// </summary>
        public enum ActionType { Fold, Check, Bet, Call, Raise }

        /// <summary>
        /// The format which an action is stored as
        /// </summary>
        public struct Action
        {
            /// <summary>
            /// The player that sent the action
            /// </summary>
            public Player player { get; set; }

            /// <summary>
            /// The type of action
            /// </summary>
            public ActionType type { get; set; }

            /// <summary>
            /// The value associated with the action, for example the ammount
            /// you are betting
            /// </summary>
            public float? value { get; set; }

            public override string ToString()
            {
                string actionString = $"{player.ToString()} Chose to {type.ToString()}";

                if (value != null)
                {
                    actionString += " " + value.ToString();
                }

                return actionString;
            }
        }

        public struct PlayerMessage
        {
            public Player Player { get; set; }
            public string Message { get; set; }

            public override string ToString()
            {
                return Player.Name + ": " + Message;
            }
        }

        /// <summary>
        /// Adds an action to the queue to be processed by the game manager
        /// </summary>
        /// <param name="action">The action</param>
        public void AddAction(Action action)
        {
            ActionExecuteQueue.Enqueue(action);
            MessageQueue.Enqueue(action.ToString());

            state.MessageCount = MessageQueue.Count;
        }

        /// <summary>
        /// Allows a player to display a public message
        /// </summary>
        /// <param name="message">The message</param>
        public void AddPlayerMessage(PlayerMessage message)
        {
            MessageQueue.Enqueue(message.ToString());

            state.MessageCount = MessageQueue.Count();
        }

        private Queue<Action> ActionExecuteQueue { get; set; }
        private Queue<string> MessageQueue { get; set; }

        private void ExecuteAction(Action action)
        {
            switch (action.type)
            {
                case ActionType.Fold:
                    action.player.Folded = true;
                    break;
                case ActionType.Check: break;
                case ActionType.Bet:
                    action.player.CurrentBet = action.value ?? 0;
                    break;
                case ActionType.Call: break;
                case ActionType.Raise: break;

                default:
                    throw new Exception("Unknown action type");
            }
        }

        public void ExecuteActions()
        {
            state.CurrentTurnState = GameState.TurnState.Executing;

            while (ActionExecuteQueue.Count > 0)
            {
                Action action = ActionExecuteQueue.Dequeue();
                ExecuteAction(action);
            }

            state.CurrentTurnState = GameState.TurnState.Ready;
        }

        /// <summary>
        /// Gets the message count at any given time, this information is also
        /// sent with the game state.
        /// </summary>
        /// <returns>The number of messages in the queue</returns>
        public int GetMessageCount()
        {
            return MessageQueue.Count;
        }

        /// <summary>
        /// Gets the first message from the queue and removes it
        /// </summary>
        /// <returns>The message as a string</returns>
        public string GetMessage()
        {
            if (MessageQueue.Count > 0)
            {
                state.MessageCount = MessageQueue.Count - 1;

                return MessageQueue.Dequeue();
            }

            return string.Empty;
        }

        #endregion

        #region Singleton

        private GameManager()
        {
            state = new GameState();

            RegisteredPlayers = new List<Player>();
            Deck = new Stack<Card>();

            ActionExecuteQueue = new Queue<Action>();
            MessageQueue = new Queue<string>();
        }

        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameManager();
                }
                return instance;
            }
        }

        #endregion

        #region Rounds and Game Logic

        private Player GetPlayerAfterDealer(int dealerIndex, int n)
        {
            return RegisteredPlayers[(dealerIndex + n) % RegisteredPlayers.Count];
        }

        private void Round()
        {
            if (state.Dealer == null)
            {
                state.Dealer = RegisteredPlayers.FirstOrDefault();
            }

            int dealerIndex = (RegisteredPlayers.IndexOf(state.Dealer) + 1) % RegisteredPlayers.Count;
            state.Dealer = RegisteredPlayers[dealerIndex];

            Deal();

            // Small and Large Blind
            for (int i = 0; i < 2; i++)
            {

            }
        }
        #endregion
    }
}
