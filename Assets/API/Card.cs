using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Poker
{
    public class Card : IComparable<Card>
    {
        public static Dictionary<Card, Sprite> CardImageCache = new();

        public enum Suits { Hearts, Diamonds, Spades, Clubs }
        public enum Ranks
        {
            Two = 2,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Ten,
            Jack,
            Queen,
            King,
            Ace
        }

        public Card(Suits suit, Ranks rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public Suits Suit { get; private set; }
        public Ranks Rank { get; private set; }

        public int CompareTo(Card other)
        {
            if (other == null) return 1;
            return Suit.CompareTo(other.Suit);
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }

        public string ToFileName()
        {
            if (Rank >= Ranks.Jack && Rank < Ranks.Ace)
            {
                return ($"{Rank}_of_{Suit}2").ToLower();
            }

            return ($"{(Rank == Ranks.Ace ? Rank : (int)Rank)}_of_{Suit}").ToLower();
        }

        public Sprite GetSprite()
        {
            if (!CardImageCache.ContainsKey(this))
            {
                string filePath = "Images/cards/" + ToFileName();
                Sprite sprite = Resources.Load<Sprite>(filePath);

                if (sprite == null)
                {
                    throw new Exception("Could not find card image at: " + filePath);
                }

                CardImageCache.Add(this, sprite);
            }

            return CardImageCache[this];
        }
    }
}