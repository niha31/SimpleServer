using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerForm
{
    public enum GameState
    {
        GS_INVITATION_SENT,
        GS_ACTIVE,
        GS_GAMEOVER
    }

    public enum GameType
    {
        GT_RPS,
        GT_SNAKE
    }

    public class Game
    {
        public GameState state;
        public GameType type;
        public int clientID1;
        public int clientID2;
        public int client1Score;
        public int client2Score;
        public string client1Input;
        public string client2Input;
        public Game(GameType type, int clientID1, int clientID2)
        {
            this.state = GameState.GS_INVITATION_SENT;
            this.type = type;
            this.clientID1 = clientID1;
            this.clientID2 = clientID2;
            this.client1Score = 0;
            this.client2Score = 0;
            this.client1Input = null;
            this.client2Input = null;
        }


        public void score(string player)
        {
            if(player == "1")
            {
                client1Score++;
            }
            if(player == "2")
            {
                client2Score++;
            }
        }

        public int ReturnScore(string player)
        {
            if (player == "1")
            {
                return client1Score;
            }
            else if (player == "2")
            {
                return client2Score;
            }
            else
            {
                return 0;
            }
        }
    }
}
