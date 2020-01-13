using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace PacketData
{
    public enum PacketType
    {
        EMPTY,
        LOGIN,
        CHATMESSAGE,
        NICKNAME,
        GAME,
        SNAKEGAME,
        SCORE,
        APPLEPOS,
        PLAYER,
        STOPGAME,
        QUIT
    }

    [Serializable]
    public class Packet
    {
        public PacketType type = PacketType.EMPTY;
    }

    [Serializable]
    public class LoginPacket : Packet
    {
        public IPEndPoint endPoint;

        public LoginPacket(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
            this.type = PacketType.LOGIN;
        }
    }


    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public string message = String.Empty;

        public ChatMessagePacket(String message)
        {
            this.type = PacketType.CHATMESSAGE;
            this.message = message;
        }
    }

    [Serializable]
    public class NickNamePakcet : Packet
    {
        public string nickName = String.Empty;

        public NickNamePakcet(string nickName)
        {
            this.type = PacketType.NICKNAME;
            this.nickName = nickName;
        }
    }

    [Serializable]
    public class QuitPacket : Packet
    {
        public string message = String.Empty;

        public QuitPacket()
        {
            message = String.Empty;
            this.type = PacketType.QUIT;
        }

        public QuitPacket(string message)
        {
            this.message = message + " has left the chat.";
            this.type = PacketType.QUIT;
        }
    }

    [Serializable]
    public class GamePacket : Packet
    {
        public GamePacket()
        {
            this.type = PacketType.GAME;
        }


    }

    [Serializable]
    public class GameMovement : Packet
    {
        public string input = String.Empty;
        public GameMovement(string input)
        {
            this.input = input;
            this.type = PacketType.SNAKEGAME;
        }
    }

    [Serializable]
    public class Score : Packet
    {
        public string input;

        public Score(string input)
        {
            this.input = input;
            this.type = PacketType.SCORE;
        }
    }

    [Serializable]
    public class ApplePos : Packet
    {
        public string pos;

        public ApplePos(string pos)
        {
            this.pos = pos;
            this.type = PacketType.APPLEPOS;
        }
    }

    [Serializable]
    public class Player : Packet
    {
        public int player;

        public Player(int player)
        {
            this.player = player;
            this.type = PacketType.PLAYER;
        }
    }

    [Serializable]
    public class StopGame : Packet
    { 
        public StopGame()
        {
            this.type = PacketType.STOPGAME;
        }
    }


}

