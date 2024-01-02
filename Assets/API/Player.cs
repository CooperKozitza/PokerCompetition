using System.Collections;
using System.Collections.Generic;
using System;
using Poker;

namespace Poker
{
    public abstract class Player
    {
        public Guid Id { get; private set; }

        public bool Dealer { get; set; }
        public bool MyTurn { get; set; }
        public bool Folded { get; set; }

        public float CurrentBet { get; set; }


        public List<Card> Hand { get; set; }
        public string Name { get; set; }

        public Player()
        {
            GameManager.Instance.RegisterPlayer(this);

            Id = new Guid();

            Dealer = false;
            MyTurn = false;
            Folded = false;

            CurrentBet = 0;

            Hand = new List<Card>();
            Name = string.Empty;
        }

        /// <summary>
        /// Sends an action to the game manager with a value. For example, you can
        /// bet with a value, but you dont need to provide a value to fold.
        /// </summary>
        /// <param name="action">The action to send</param>
        /// <param name="value">
        /// The value to send with your action, like the ammount of money to bet
        /// </param>
        /// <example>
        /// SendAction(GameManager.ActionType.Bet, 100);
        /// </example>
        public void SendAction(GameManager.ActionType actionType, float? value = null) 
        {
            GameManager.Action action = new GameManager.Action();

            action.player = this;

            action.type = actionType;
            action.value = value;

            GameManager.Instance.AddAction(action);
        }

        /// <summary>
        /// Sends a message that will appear in the UI
        /// </summary>
        /// <param name="message">The message</param>
        /// <example>
        /// SendMessage("Hello World!");
        /// </example>
        public void SendMessage(string message)
        {
            GameManager.PlayerMessage playerMessage = new GameManager.PlayerMessage();

            playerMessage.Player = this;
            playerMessage.Message = message;

            GameManager.Instance.AddPlayerMessage(playerMessage);
        }

        public override string ToString()
        {
            return Name ?? "No Name";
        }

        public abstract void OnTurn(GameManager.GameState state);
    }
}