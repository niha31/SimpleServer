using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace SimpleClient
{
    public class SimpleClient
    {
        TcpClient tcpClient = null;
        UdpClient udpClient = null;
        Stream NetworkStream;
        IPEndPoint endPoint;
        BinaryWriter writer;
        BinaryReader reader;
        public MemoryStream memoryStream = new MemoryStream();
        public BinaryFormatter binaryFormatter = new BinaryFormatter();
        public Thread Readerthread;
        public Thread UdpReaderThread;
        ClientForm messageForm;
        SnakeGame snakeGame;

        public SimpleClient()
        {
            tcpClient = new TcpClient();
            udpClient = new UdpClient();
            messageForm = new ClientForm(this);
            snakeGame = new SnakeGame(this);
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                tcpClient.Connect(ipAddress, port);
                NetworkStream = tcpClient.GetStream();
                writer = new BinaryWriter(NetworkStream);
                reader = new BinaryReader(NetworkStream);
                Readerthread = new Thread(new ThreadStart(ProcessServerResponse));
                UdpReaderThread = new Thread(new ThreadStart(UDPServerResponse));

                udpClient.Connect(ipAddress, port);

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }

            return true;
        }

        public void Run()
        {
            Application.Run(messageForm);
        }

        public void Stop()
        {
            Application.Exit();
        }

        public void TCPSendMessage(string message)
        {
            if (message != null)
            {
                writer.Write(message);
                writer.Flush();
            }
        }

        public void TCPStop()
        {
            Readerthread.Abort();
        }

        public void UDPStop()
        {
            UdpReaderThread.Abort();
        }

        public void startSnakeGame()
        {
            Application.Run(snakeGame);

        }
        public void stopSnakeGame()
        {
            snakeGame.Close();
        }

        public void ProcessServerResponse()
        {
            int numOfIncomingBytes = 0;
            while ((numOfIncomingBytes = reader.ReadInt32()) != 0)
            {
                byte[] buffer = reader.ReadBytes(numOfIncomingBytes);
                memoryStream.Write(buffer, 0, numOfIncomingBytes);
                memoryStream.Position = 0;
                binaryFormatter.Binder = new MyBinder.MyBinder();
                PacketData.Packet rawPacket = (PacketData.Packet)binaryFormatter.Deserialize(memoryStream);
                memoryStream.SetLength(0);

                switch (rawPacket.type)
                {
                    case PacketData.PacketType.CHATMESSAGE:
                        PacketData.ChatMessagePacket packet = (PacketData.ChatMessagePacket)rawPacket;
                        messageForm.UpdateChatWindow(packet.message);
                        break;
                    case PacketData.PacketType.GAME:
                        startSnakeGame();
                        break;
                    case PacketData.PacketType.SNAKEGAME:
                        Socket udpSocket = udpClient.Client;
                        PacketData.LoginPacket loginpacket = new PacketData.LoginPacket((IPEndPoint)udpSocket.LocalEndPoint);
                        TCPSend(loginpacket);
                        PacketData.GameMovement gamemovement = (PacketData.GameMovement)rawPacket;
                        snakeGame.UpdateSnake(gamemovement.input);
                        break;
                    case PacketData.PacketType.SCORE:
                        PacketData.Score score = (PacketData.Score)rawPacket;
                        snakeGame.UpdateScore(score.input);
                        break;
                    case PacketData.PacketType.APPLEPOS:
                        PacketData.ApplePos pos = (PacketData.ApplePos)rawPacket;
                        snakeGame.SetApplePos(pos.pos);
                        break;
                    case PacketData.PacketType.PLAYER:
                        PacketData.Player player = (PacketData.Player)rawPacket;
                        snakeGame.startingText(player.player);
                        break;
                    case PacketData.PacketType.STOPGAME:
                        snakeGame.Close();
                        break;
                    case PacketData.PacketType.QUIT:
                        
                        TCPStop();
                        //PacketData.QuitPacket quitPacket = (PacketData.QuitPacket)rawPacket;
                        //messageForm.UpdateChatWindow(quitPacket.message);
                        break;
                    default:

                    break;
                    
                }
            }
        }

        public void TCPSend(PacketData.Packet data)
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

            udpClient.Send(buffer, buffer.Length);
        }

        void UDPServerResponse()
        {

            //IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //Byte[] receiveBytes = udpClient.Receive(ref remoteIpEndPoint);

            int numOfIncomingBytes = 0;
            byte[] bytes = new byte[256];

            while ((numOfIncomingBytes = reader.ReadInt32()) != 0)
            {
                byte[] buffer = reader.ReadBytes(numOfIncomingBytes);
                memoryStream.Write(buffer, 0, numOfIncomingBytes);

                memoryStream.Position = 0;
                binaryFormatter.Binder = new MyBinder.MyBinder();
                PacketData.Packet rawPacket = (PacketData.Packet)binaryFormatter.Deserialize(memoryStream);
                memoryStream.SetLength(0);

                switch (rawPacket.type)
                {
                    case PacketData.PacketType.LOGIN:
                        PacketData.LoginPacket loginPacket = (PacketData.LoginPacket)rawPacket;
                        HandlePacket(loginPacket.endPoint);
                        break;
                    case PacketData.PacketType.CHATMESSAGE:
                        PacketData.ChatMessagePacket packet = (PacketData.ChatMessagePacket)rawPacket;
                        messageForm.UpdateChatWindow(packet.message);
                        break;
                    case PacketData.PacketType.GAME:
                        startSnakeGame();
                        break;
                    case PacketData.PacketType.SNAKEGAME:
                        PacketData.GameMovement gameMovement = (PacketData.GameMovement)rawPacket;
                        snakeGame.UpdateSnake(gameMovement.input);
                        break;
                    case PacketData.PacketType.SCORE:
                        PacketData.Score score = (PacketData.Score)rawPacket;
                        snakeGame.UpdateScore(score.input);
                        break;
                    case PacketData.PacketType.APPLEPOS:
                        PacketData.ApplePos pos = (PacketData.ApplePos)rawPacket;
                        snakeGame.SetApplePos(pos.pos);
                        break;
                    case PacketData.PacketType.PLAYER:
                        PacketData.Player player = (PacketData.Player)rawPacket;
                        snakeGame.startingText(player.player);
                        break;
                    case PacketData.PacketType.STOPGAME:
                        snakeGame.CloseForm();
                        //PacketData.StopGame stopgame = new PacketData.StopGame();
                        //TCPSend(stopgame);
                        //snakeGame.Close();
                        break;
                    case PacketData.PacketType.QUIT:
                        UDPStop();
                        break;
                }
            }            
        }

        void HandlePacket(IPEndPoint endPoint)
        {
            udpClient.Connect(endPoint);
            Socket udpSocket = udpClient.Client;

            UdpReaderThread = new Thread(new ThreadStart(UDPServerResponse));
            UdpReaderThread.Start();
        }
    }
}