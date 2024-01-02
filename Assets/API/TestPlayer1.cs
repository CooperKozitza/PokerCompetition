using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace Poker
{
    public class TestPlayer1 : Player
    { 
        public TestPlayer1()
        {
            Name = "Player 1";
        }

        public override void OnTurn(GameManager.GameState state)
        {
            SendAction(GameManager.ActionType.Bet, 15);
            SendAction(GameManager.ActionType.Fold);
        }
    }

    public class TestPlayer2 : Player
    {
        public TestPlayer2()
        {
            Name = "Player 2";
        }

        public override void OnTurn(GameManager.GameState state)
        {
            SendAction(GameManager.ActionType.Bet, 10);
            SendAction(GameManager.ActionType.Raise, 12);
            SendAction(GameManager.ActionType.Fold);
        }
    }

    public class TestPlayer3 : Player
    {
        public TestPlayer3()
        {
            Name = "Player 3";
        }

        public override void OnTurn(GameManager.GameState state)
        {
            SendAction(GameManager.ActionType.Bet, 15);
        }
    }
}