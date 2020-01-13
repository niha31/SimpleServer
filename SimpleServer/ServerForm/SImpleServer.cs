using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerForm
{
    class SimpleServer
    {
        private TcpListener tcpListener = null;
        List<Client> clients;
        private int backlog;
        public List<Game> games;
        public MemoryStream memoryStream = new MemoryStream();
        public BinaryFormatter binaryFormatter = new BinaryFormatter();

        public SimpleServer(string ipAddress, int port)
        {
            IPAddress address = IPAddress.Parse(ipAddress);

            tcpListener = new TcpListener(address, port);

            clients = new List<Client>();
            games = new List<Game>();

            Start();
        }

        public void Start()
        {
            while (true)
            {
                tcpListener.Start(backlog);
                Console.WriteLine("started listening");

                Socket tcpSocket = tcpListener.AcceptSocket();
                Client client = new Client(tcpSocket);
                client.id = clients.Count;
                client.name = "Client " + client.id.ToString();
                clients.Add(client);

                Console.WriteLine("Connection Accepted");

                foreach (Client element in clients)
                {
                    PacketData.ChatMessagePacket packet = new PacketData.ChatMessagePacket(client.name + " joined the chat");
                    send(packet, element);
                }

                Thread tcp = new Thread(new ParameterizedThreadStart(TCPClientMethod));
                tcp.Start(client);
            }
        }

        public void Stop()
        {
            tcpListener.Stop();
        }

        void UDPClientMethod(object clientObj)
        {
            int numOfIncomingBytes = 0;
            Client client = (Client)clientObj;

            byte[] bytes = new byte[256];
            while((numOfIncomingBytes = client.reader.ReadInt32()) != 0)
            {
                byte[] buffer = client.reader.ReadBytes(numOfIncomingBytes);
                memoryStream.Position = 0;

                IPEndPoint endpoint = (IPEndPoint)(client.udpSocket.RemoteEndPoint);
                int receiveBytes = client.udpSocket.Receive(buffer);
                memoryStream.Write(buffer, 0, receiveBytes);

                binaryFormatter.Binder = new MyBinder.MyBinder();
                PacketData.Packet rawPacket = (PacketData.Packet)binaryFormatter.Deserialize(memoryStream);

                memoryStream.SetLength(0);

                switch (rawPacket.type)
                {
                    case PacketData.PacketType.LOGIN:
                        PacketData.LoginPacket packet = (PacketData.LoginPacket)rawPacket;
                        HandlePacket(client, packet.endPoint);
                    break;
                    case PacketData.PacketType.CHATMESSAGE:
                        PacketData.ChatMessagePacket chatMessage = (PacketData.ChatMessagePacket)rawPacket;

                        if (chatMessage.message.Substring(0, 1) == "/")
                        {
                            Commands(chatMessage, client);
                        }
                        if (client.gameID != -1 && games[client.gameID].state == GameState.GS_ACTIVE)
                        {
                            if (games[client.gameID].type == GameType.GT_RPS)
                            {
                                if (client.id == games[client.gameID].clientID1 && games[client.gameID].client1Input == null)
                                {
                                    games[client.gameID].client1Input = chatMessage.message;
                                }
                                if (client.id == games[client.gameID].clientID2 && games[client.gameID].client2Input == null)
                                {
                                    games[client.gameID].client2Input = chatMessage.message;
                                }
                                PlayRPS(games[client.gameID].clientID1, games[client.gameID].clientID2);
                            }
                        }
                        else
                        {
                            foreach (Client element in clients)
                            {
                                if (element.gameID == -1)
                                {
                                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket(client.name + ": " + chatMessage.message);
                                    UDPSend(packet1, element);
                                }
                            }
                        }
                        break;
                    case PacketData.PacketType.SNAKEGAME:
                        foreach (Game element in games)
                        {
                            if (element.clientID1 == client.id)
                            {
                                PacketData.GameMovement gamePacket = (PacketData.GameMovement)rawPacket;
                                PlaySnake(element.clientID1, element.clientID2, gamePacket.input, "1");
                            }
                            else if (element.clientID2 == client.id)
                            {
                                PacketData.GameMovement gamePacket = (PacketData.GameMovement)rawPacket;
                                PlaySnake(element.clientID1, element.clientID2, gamePacket.input, "2");
                            }
                        }
                        break;
                    case PacketData.PacketType.NICKNAME:
                        PacketData.NickNamePakcet nickNamePacket = (PacketData.NickNamePakcet)rawPacket;
                        PacketData.ChatMessagePacket newPacket = new PacketData.ChatMessagePacket(client.Rename(client, nickNamePacket.nickName));

                        foreach (Client element in clients)
                        {
                            UDPSend(newPacket, element);
                        }

                        break;
                    case PacketData.PacketType.STOPGAME:
                        PacketData.QuitPacket stop = (PacketData.QuitPacket)rawPacket;
                        send(stop, client);
                        break;
                }
            }
        }

        void HandlePacket(object clientObj, IPEndPoint endPoint)
        {
            Client client = (Client)clientObj;
            client.UDPConnect(endPoint);

            Thread udp = new Thread(new ParameterizedThreadStart(UDPClientMethod));
            udp.Start(client);
        }

        void TCPClientMethod(object clientObj)
        {
            int numOfIncomingBytes = 0;
            Client client = (Client)clientObj;

            while ((numOfIncomingBytes = client.reader.ReadInt32()) != 0)
            {
                byte[] buffer = client.reader.ReadBytes(numOfIncomingBytes);
                memoryStream.Write(buffer, 0, numOfIncomingBytes);
                memoryStream.Position = 0;

                binaryFormatter.Binder = new MyBinder.MyBinder();
                PacketData.Packet rawPacket = (PacketData.Packet)binaryFormatter.Deserialize(memoryStream);

                memoryStream.SetLength(0);
                switch (rawPacket.type)
                {
                    case PacketData.PacketType.CHATMESSAGE:
                        PacketData.ChatMessagePacket packet = (PacketData.ChatMessagePacket)rawPacket;

                        if (packet.message.Substring(0, 1) == "/")
                        {
                            Commands(packet, client);
                        }
                        if (client.gameID != -1 && games[client.gameID].state == GameState.GS_ACTIVE)
                        {
                            if (games[client.gameID].type == GameType.GT_RPS)
                            {
                                if (client.id == games[client.gameID].clientID1 && games[client.gameID].client1Input == null)
                                {
                                    games[client.gameID].client1Input = packet.message;
                                }
                                if (client.id == games[client.gameID].clientID2 && games[client.gameID].client2Input == null)
                                {
                                    games[client.gameID].client2Input = packet.message;
                                }
                                PlayRPS(games[client.gameID].clientID1, games[client.gameID].clientID2);
                            }
                        }
                        else
                        {
                            foreach (Client element in clients)
                            {
                                if (element.gameID == -1)
                                {
                                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket(client.name + ": " + packet.message);
                                    send(packet1, element);
                                }
                            }
                        }
                    break;
                    case PacketData.PacketType.NICKNAME:
                        PacketData.NickNamePakcet nickNamePacket = (PacketData.NickNamePakcet)rawPacket;
                        PacketData.ChatMessagePacket newPacket = new PacketData.ChatMessagePacket(client.Rename(client, nickNamePacket.nickName));

                        foreach (Client element in clients)
                        {
                            send(newPacket, element);
                        }

                    break;
                    case PacketData.PacketType.SNAKEGAME:
                        
                        foreach (Game element in games)
                        {
                            if(element.clientID1 == client.id)
                            {
                                PacketData.GameMovement gamePacket = (PacketData.GameMovement)rawPacket;
                                PlaySnake(element.clientID1, element.clientID2, gamePacket.input, "1");
                            }
                            else if (element.clientID2 == client.id)
                            {
                                PacketData.GameMovement gamePacket = (PacketData.GameMovement)rawPacket;
                                PlaySnake(element.clientID1, element.clientID2, gamePacket.input, "2");
                            }
                        }

                    break;
                    case PacketData.PacketType.SCORE:
                         foreach (Game element in games)
                         {
                            if (element.clientID1 == client.id || element.clientID2 == client.id)
                            {
                                PacketData.Score scorepacket = (PacketData.Score)rawPacket;
                                SnakeScore(element.clientID1, element.clientID2, scorepacket, client);
                            }
                         }
                        break;
                    case PacketData.PacketType.APPLEPOS:
                        foreach (Game element in games)
                        {
                            if (element.clientID1 == client.id || element.clientID2 == client.id)
                            {
                                SnakeGameApple(element.clientID1, element.clientID2);
                            }
                           
                        }
                        break;
                    case PacketData.PacketType.PLAYER:
                        foreach (Game element in games)
                        {
                            if(element.clientID1 == client.id )
                            {
                                PacketData.Player playerPacket = new PacketData.Player(1);
                                send(playerPacket, client);
                            }
                            else if(element.clientID2 == client.id)
                            {
                                PacketData.Player playerPacket = new PacketData.Player(2);
                                send(playerPacket, client);
                            }

                        }
                        break;
                    case PacketData.PacketType.LOGIN:
                        PacketData.LoginPacket login = (PacketData.LoginPacket)rawPacket;
                        HandlePacket(client, login.endPoint);
                        break;
                    case PacketData.PacketType.STOPGAME:
                        PacketData.StopGame stop = (PacketData.StopGame)rawPacket;
                        send(stop, client);
                        break;
                    case PacketData.PacketType.QUIT:

                        PacketData.QuitPacket quit = (PacketData.QuitPacket)rawPacket;
                        string message = client.name + " " + quit.message;
                        PacketData.ChatMessagePacket newMessage = new PacketData.ChatMessagePacket(message);
                        //PacketData.QuitPacket quitMessage = new PacketData.QuitPacket(client.name + quit.message);
                        //send(quitMessage, client);
                        client.tcpSocket.Close();
                        clients.Remove(client);

                        foreach (Client element in clients)
                        {
                            send(newMessage, element);
                        }
                        break;
                }
            }                                  
        }


        void Commands(PacketData.ChatMessagePacket packet, Client client)
        {
            string command;
            int space = -1;

            if (packet.message.Contains(" "))
            {
                space = packet.message.IndexOf(" ");
                command = packet.message.Substring(0, space);
            }
            else
            {
                command = packet.message;
            }

            if (command == "/rps")
            {
                string name = packet.message.Substring(space + 1, packet.message.Length - 5);
                RPS(name, client);
            }
            else if(command == "/snakeGame")
            {
                string name = packet.message.Substring(space + 1, packet.message.Length - 11);
                Snake(name, client);
            }
            else if (command == "/accept")
            {
                if (client.gameID == -1)
                {
                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("You have nothing to accept");
                    send(packet1, client);
                }
                else
                {
                    games[client.gameID].state = GameState.GS_ACTIVE;
                    if (games[client.gameID].type == GameType.GT_RPS)
                    {
                        PlayRPS(games[client.gameID].clientID1, games[client.gameID].clientID2);
                    }
                    else if(games[client.gameID].type == GameType.GT_SNAKE)
                    {
                        StartSnake(games[client.gameID].clientID1, games[client.gameID].clientID2);
                    }
                }
            }
            else if (command == "/decline")
            {
                if (client.gameID != -1)
                {
                    foreach (Client element in clients)
                    {
                        if (element.gameID == client.gameID)
                        {
                            PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("Game has been declined");
                            send(packet1, element);
                        }
                    }

                    if (client.id == games[client.gameID].clientID1)
                    {
                        clients[games[client.gameID].clientID2].gameID = -1;
                        games.Remove(games[client.gameID]);
                        client.gameID = -1;
                    }
                    else if (client.id == games[client.gameID].clientID2)
                    {
                        clients[games[client.gameID].clientID1].gameID = -1;
                        games.Remove(games[client.gameID]);
                        client.gameID = -1;
                    }
                }
                else
                {
                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("You have nothing to decline");
                    send(packet1, client);
                }
            }
            else if (command == "/continue")
            {
                if (client.gameID != -1)
                {
                    games[client.gameID].state = GameState.GS_ACTIVE;
                    PlayRPS(games[client.gameID].clientID1, games[client.gameID].clientID2);
                }
                else
                {
                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("You have noting to continue");
                    send(packet1, client);
                }
            }
            else if (command == "/stop")
            {
                if (client.gameID != -1)
                {
                    if (games[client.gameID].type == GameType.GT_RPS)
                    {
                        PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("You have stopped playing RPS");
                        send(packet1, client);

                        packet1 = new PacketData.ChatMessagePacket(client.name + " has to stop playing RPS.  You have stopped playing RPS");
                        foreach (Client element in clients)
                        {
                            if (client.gameID == element.gameID && client.id != element.id)
                            {
                                send(packet1, element);
                            }
                        }

                        if (client.id == games[client.gameID].clientID1)
                        {
                            clients[games[client.gameID].clientID2].gameID = -1;
                            games.Remove(games[client.gameID]);
                            client.gameID = -1;
                        }
                        else if (client.id == games[client.gameID].clientID2)
                        {
                            clients[games[client.gameID].clientID1].gameID = -1;
                            games.Remove(games[client.gameID]);
                            client.gameID = -1;
                        }
                    }
                    if (games[client.gameID].type == GameType.GT_SNAKE)
                    {

                       

                        PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("You have stopped playing snake");
                        send(packet1, client);

                        packet1 = new PacketData.ChatMessagePacket(client.name + " has to stop playing snake.  You have stopped playing RPS");
                        foreach (Client element in clients)
                        {
                            if (client.gameID == element.gameID && client.id != element.id)
                            {
                                send(packet1, element);
                            }
                        }

                        PacketData.StopGame stop = new PacketData.StopGame();
                        send(stop, clients[games[client.gameID].clientID1]);
                        send(stop, clients[games[client.gameID].clientID2]);

                        if (client.id == games[client.gameID].clientID1)
                        {
                            clients[games[client.gameID].clientID2].gameID = -1;
                            games.Remove(games[client.gameID]);
                            client.gameID = -1;
                        }
                        else if (client.id == games[client.gameID].clientID2)
                        {
                            clients[games[client.gameID].clientID1].gameID = -1;
                            games.Remove(games[client.gameID]);
                            client.gameID = -1;
                        }


                    }
                }
                else
                {
                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("You have nothing to stop");
                    send(packet1, client);
                }
            }
            else
            {
                PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("INVALID COMMAND");
                send(packet1, client);
            }
        }


        public void send(PacketData.Packet data, Client client)
        {
            binaryFormatter.Serialize(memoryStream, data);
            memoryStream.Flush();
            byte[] buffer = memoryStream.GetBuffer();
            memoryStream.SetLength(0);

            client.writer.Write(buffer.Length);
            client.writer.Write(buffer);
            client.writer.Flush();
        }

        public void UDPSend(PacketData.Packet packet, Client client)
        {
            binaryFormatter.Serialize(memoryStream, packet);
            memoryStream.Flush();
            byte[] buffer = memoryStream.GetBuffer();
            memoryStream.SetLength(0);

            client.udpSocket.SendTo(buffer, client.udpSocket.LocalEndPoint);
        }

        void RPS(string name, Client client)
        {
            bool found = false;
            foreach (Client element in clients)
            {
                if (element.name == name && element.gameID == -1)
                {
                    found = true;
                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("You have challanged " + element.name);
                    send(packet1, client);
                    packet1 = new PacketData.ChatMessagePacket(client.name + " has challanged you to a game of Rock, Paper, Scissors. Do you accept? /accept or /decline");
                    send(packet1, element);

                    games.Add(new Game(GameType.GT_RPS, client.id, element.id));

                    client.gameID = games.Count - 1;
                    element.gameID = games.Count - 1;
                }
            }

            if (found == false)
            {
                PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("Person you are trying to play with is either in a game or do not exist");
                send(packet1, client);
            }
        }

        void PlayRPS(int clientID1, int clientID2)
        {
            string rock = "rock";
            string paper = "paper";
            string scissors = "scissors";

            string player1 = games[clients[clientID1].gameID].client1Input;
            string player2 = games[clients[clientID1].gameID].client2Input;

            if (games[clients[clientID1].gameID].state == GameState.GS_ACTIVE)
            {
                if (player1 == null || player2 == null)
                {
                    if (player1 == null && player2 == null)
                    {
                        PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("Rock, Paper or Scissors");
                        send(packet1, clients[clientID1]);

                        packet1 = new PacketData.ChatMessagePacket("Rock, Paper or Scissors");
                        send(packet1, clients[clientID2]);
                    }
                }

                else if (player1 == player2)
                {
                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("Draw. You both picked " + player1);
                    send(packet1, clients[clientID1]);

                    packet1 = new PacketData.ChatMessagePacket("Draw. You both picked " + player2);
                    send(packet1, clients[clientID2]);

                    games[clients[clientID1].gameID].state = GameState.GS_GAMEOVER;
                }
                else if (player1 == rock && player2 == scissors || player1 == paper && player2 == rock || player1 == scissors && player2 == paper)
                {
                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("You Win. You picked " + player1 + " and " + clients[clientID2].name + " picked " + player2);
                    send(packet1, clients[clientID1]);

                    packet1 = new PacketData.ChatMessagePacket("You Lose. You picked " + player2 + " and " + clients[clientID1].name + " picked " + player1);
                    send(packet1, clients[clientID2]);

                    games[clients[clientID1].gameID].client1Score++;

                    games[clients[clientID1].gameID].state = GameState.GS_GAMEOVER;
                }
                else if (player2 == rock && player1 == scissors || player2 == paper && player1 == rock || player2 == scissors && player1 == paper)
                {
                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("You Lose. You picked " + player1 + " and " + clients[clientID2].name + " picked " + player2);
                    send(packet1, clients[clientID1]);

                    packet1 = new PacketData.ChatMessagePacket("You Win. You picked " + player2 + " and " + clients[clientID1].name + " picked " + player1);
                    send(packet1, clients[clientID2]);

                    games[clients[clientID2].gameID].client2Score++;

                    games[clients[clientID1].gameID].state = GameState.GS_GAMEOVER;
                }
                else
                {
                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("Someone played a invalid option. please re-enter choice");
                    send(packet1, clients[clientID1]);

                    packet1 = new PacketData.ChatMessagePacket("Someone played a invalid option. please re-enter choice");
                    send(packet1, clients[clientID2]);

                    games[clients[clientID1].gameID].client1Input = null;
                    games[clients[clientID2].gameID].client2Input = null;
                }

                if (games[clients[clientID1].gameID].state == GameState.GS_GAMEOVER)
                {
                    games[clients[clientID1].gameID].client1Input = null;
                    games[clients[clientID2].gameID].client2Input = null;

                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("Points: " + clients[clientID1].name + ": " + games[clients[clientID1].gameID].client1Score + " " + clients[clientID2].name + ": " + games[clients[clientID1].gameID].client2Score);
                    send(packet1, clients[clientID1]);

                    packet1 = new PacketData.ChatMessagePacket("Points: " + clients[clientID1].name + ": " + games[clients[clientID1].gameID].client1Score + " " + clients[clientID2].name + ": " + games[clients[clientID1].gameID].client2Score);
                    send(packet1, clients[clientID2]);

                    packet1 = new PacketData.ChatMessagePacket("To stop playing enter /stop or to contiue enter /continue");
                    send(packet1, clients[clientID1]);

                    packet1 = new PacketData.ChatMessagePacket("To stop playing enter /stop or to contiue enter /continue");
                    send(packet1, clients[clientID2]);
                }


            }

        }
        void Snake(string name, Client client)
        {
            bool found = false;
            foreach (Client element in clients)
            {
                if (element.name == name && element.gameID == -1)
                {
                    found = true;
                    PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("You have challanged " + element.name);
                    send(packet1, client);
                    packet1 = new PacketData.ChatMessagePacket(client.name + " has challanged you to a game of Snake. Do you accept? /accept or /decline");
                    send(packet1, element);

                    games.Add(new Game(GameType.GT_SNAKE, client.id, element.id));

                    client.gameID = games.Count - 1;
                    element.gameID = games.Count - 1;
                }
            }

            if (found == false)
            {
                PacketData.ChatMessagePacket packet1 = new PacketData.ChatMessagePacket("Person you are trying to play with is either in a game or do not exist");
                send(packet1, client);
            }
        }
        void StartSnake(int clientID1, int clientID2)
        {
            SnakeGameApple(clientID1, clientID2);
            PacketData.GamePacket packet1 = new PacketData.GamePacket();
            send(packet1, clients[clientID1]);
            send(packet1, clients[clientID2]);
        }

        void PlaySnake(int clientID1, int clientID2, string input, string whichPlayer)
        {
            if(input == "W")
            {
                PacketData.GameMovement packet = new PacketData.GameMovement("W" + whichPlayer);
                send(packet, clients[clientID1]);
                send(packet, clients[clientID2]);         
            }
            if (input == "S")
            {
                PacketData.GameMovement packet = new PacketData.GameMovement("S" + whichPlayer);
                send(packet, clients[clientID1]);
                send(packet, clients[clientID2]);
            }
            if (input == "D")
            {
                PacketData.GameMovement packet = new PacketData.GameMovement("D" + whichPlayer);
                send(packet, clients[clientID1]);
                send(packet, clients[clientID2]);
            }
            if (input == "A")
            {
                PacketData.GameMovement packet = new PacketData.GameMovement("A" + whichPlayer);
                send(packet, clients[clientID1]);
                send(packet, clients[clientID2]);
            }
            if(input == "P")
            {
                PacketData.GameMovement packet = new PacketData.GameMovement("P" + whichPlayer);
                send(packet, clients[clientID1]);
                send(packet, clients[clientID2]);
            }
        }

        void SnakeScore(int clientid1, int clientid2, PacketData.Score input, object clientObj)
        {
            Client client = (Client)clientObj;

            string inputStr = input.input.ToString();
            string player = inputStr.Substring(inputStr.IndexOf(".") + 1);

            games[client.gameID].score(player);

            if (player == "1")
            {
                int score = games[client.gameID].ReturnScore(player);
                PacketData.Score scorePacket = new PacketData.Score( score.ToString() + "." + player);
                send(scorePacket, clients[clientid1]);
                send(scorePacket, clients[clientid2]);
            }
            else if (player == "2")
            {
                int score = games[client.gameID].ReturnScore(player);
                PacketData.Score scorePacket = new PacketData.Score(score + "." + player);
                send(scorePacket, clients[clientid1]);
                send(scorePacket, clients[clientid2]);
            }
        }

        void SnakeGameApple(int clientID1, int clientID2)
        {
            Random rnd = new Random();
            string position = (rnd.Next(20, 500).ToString() + "." + rnd.Next(20, 350).ToString());
            PacketData.ApplePos posPacket = new PacketData.ApplePos(position);
            send(posPacket, clients[clientID1]);
            send(posPacket, clients[clientID2]);
        }
    }




    class Client
    {
        public Socket tcpSocket;
        public Socket udpSocket;
        public MemoryStream memoryStream = new MemoryStream();
        public BinaryFormatter binaryFormatter = new BinaryFormatter();
        NetworkStream stream;
        public BinaryReader reader;
        public BinaryWriter writer;
        public int id;
        public string name;
        public int gameID = -1;


        public Client(Socket _tcpSocket)
        {
            tcpSocket = _tcpSocket;

            stream = new NetworkStream(tcpSocket);

            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);

            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void UDPConnect(EndPoint clientConnection)
        {
            udpSocket.Connect(clientConnection);
            PacketData.LoginPacket loginPacket = new PacketData.LoginPacket((IPEndPoint)udpSocket.LocalEndPoint);

            TCPSend(loginPacket);
        }


        void TCPSend(PacketData.Packet data)
        {
            binaryFormatter.Serialize(memoryStream, data);
            memoryStream.Flush();
            byte[] buffer = memoryStream.GetBuffer();
            memoryStream.SetLength(0);

            writer.Write(buffer.Length);
            writer.Write(buffer);
            writer.Flush();
        }

        public void UDPSend(PacketData.Packet packet)
        {
            binaryFormatter.Serialize(memoryStream, packet);
            memoryStream.Flush();
            byte[] buffer = memoryStream.GetBuffer();
            memoryStream.SetLength(0);

            udpSocket.Send(buffer);
        }

        public void Close()
        {
            tcpSocket.Close();
        }

        public string Rename(Client client, string name)
        {
            client.name = name;
            string message = client.name + ": CLIENT CHANGED NAME";

            return message;
        }
    }
}


