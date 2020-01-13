using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Configuration;

namespace SimpleClient
{
    public partial class SnakeGame : Form
    {
        delegate void UpdateSnakeDelegate(string movement);
        UpdateSnakeDelegate _updateSnakeDelegate;
        SimpleClient _client;

        Image _snake1;
        Image _snake2;
        Image _apple;
        Rectangle _rect1;
        Rectangle _rect2;
        Rectangle _appleRect;
        string facing1 = "P";
        string facing2 = "P";
        Graphics g1;
        Graphics g2;
        Graphics gApple;
        Random rnd = new Random();

        Point point1 = new Point(20, 400);
        string score1 = "Player 1 Score: 0";

        string score2 = "Player 2 Score: 0";
        Point point2 = new Point(400, 400);



        public SnakeGame(SimpleClient client)
        {
            _client = client;
            InitializeComponent();

            _updateSnakeDelegate = new UpdateSnakeDelegate(UpdateSnake);

            _snake1 = Image.FromFile("C:/Users/nihar/OneDrive/Desktop/New folder/SimpleServer/SimpleServer/Images/BlueSnake1.bmp");
            _snake2 = Image.FromFile("C:/Users/nihar/OneDrive/Desktop/New folder/SimpleServer/SimpleServer/Images/redSnake.bmp");
            _apple = Image.FromFile("C:/Users/nihar/OneDrive/Desktop/New folder/SimpleServer/SimpleServer/Images/apple.bmp");
            _rect1 = new Rectangle(20, 20, 70, 70);
            _rect2 = new Rectangle(500, 20, 70, 70);
            _appleRect = new Rectangle();
            //_appleRect = new Rectangle(rnd.Next(20, 500), rnd.Next(20, 350), 20, 20);

            //drawFont.Dispose();
            //drawBrush.Dispose();
            //player.Dispose();
        }

        private void SnakeGame_Load(object sender, EventArgs e)
        {
            _client.UdpReaderThread.Start();
            PacketData.Player player = new PacketData.Player(1);
            _client.TCPSend(player);
        }

        public void CloseForm()
        {
            _client.UdpReaderThread.Abort();
            PacketData.StopGame stop = new PacketData.StopGame();
            _client.TCPSend(stop);
        }

        public void SetApplePos(string input)
        {
            string x = input.Substring(0, input.IndexOf("."));
            string y = input.Substring(input.IndexOf(".") + 1);

            _appleRect.X = Convert.ToInt32(x);
            _appleRect.Y = Convert.ToInt32(y);
            _appleRect.Height = 20;
            _appleRect.Width = 20;
        }

        private void SnakeGame_Paint(object sender, PaintEventArgs e)
        {
            g1 = e.Graphics;
            g1.DrawImage(_snake1, _rect1);

            g2 = e.Graphics;
            g2.DrawImage(_snake2, _rect2);

            gApple = e.Graphics;
            gApple.DrawImage(_apple, _appleRect);

            using (Font font = new Font("Times New Roman", 24, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                TextRenderer.DrawText(e.Graphics, score1, font, point1, Color.Black);

                TextRenderer.DrawText(e.Graphics, score2, font, point2, Color.Black);
            }            
        }

        public void startingText(int player)
        {
            Graphics formGraphics = this.CreateGraphics();
            string drawString = "You are Player " + player.ToString(); ;
            Font drawFont = new System.Drawing.Font("Arial", 16);
            SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            float x = 150.0F;
            float y = 50.0F;
            StringFormat drawFormat = new StringFormat();
            formGraphics.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);
            drawFont.Dispose();
            drawBrush.Dispose();
            formGraphics.Dispose();
        }

        public void UpdateSnake(string input)
        {
            string whichPlayer = input.Substring(1, 1);
            string movement = input.Substring(0, 1);
            if (movement == "W")
            {
                if (whichPlayer == "1")
                {
                    facing1 = "W";
                }
                else if (whichPlayer == "2")
                {
                    facing2 = "W";
                }
            }
            if(movement == "S")
            {
                if (whichPlayer == "1")
                {
                    facing1 = "S";

                }
                else if (whichPlayer == "2")
                {
                    facing2 = "S";
                }
            }
            if (movement == "D")
            {
                if (whichPlayer == "1")
                {
                    facing1 = "D";
                }
                else if (whichPlayer == "2")
                {
                    facing2 = "D";
                }
            }
            if (movement == "A")
            {
                if (whichPlayer == "1")
                {
                    facing1 = "A";
                }
                else if (whichPlayer == "2")
                {
                    facing2 = "A";
                }
            }
            if(movement == "P")
            {
                if (whichPlayer == "1")
                {
                    facing1 = "P";
                }
                else if (whichPlayer == "2")
                {
                    facing2 = "P";
                }
            }

        }

        public void UpdateScore(string input)
        {
            string score = input.Substring(0, input.IndexOf("."));
            string player = input.Substring(input.IndexOf(".") + 1);

            if (player == "1")
            {
                score1 = "Player 1 Score: " + score.ToString();
            }
            else if(player == "2")
            {
                score2 = "Player 2 Score: " + score.ToString();
            }           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(facing1 == "W")
            {
                _rect1.Y += 15;
            }
            if(facing1 == "S")
            {
                _rect1.Y -= 15;
            }
            if(facing1 == "A")
            {
                _rect1.X -= 15;
            }
            if(facing1 == "D")
            {
                _rect1.X += 15;
            }

            if (facing2 == "W")
            {
                _rect2.Y += 15;
            }
            if (facing2 == "S")
            {
                _rect2.Y -= 15;
            }
            if (facing2 == "A")
            {
                _rect2.X -= 15;
            }
            if (facing2 == "D")
            {
                _rect2.X += 15;
            }

            var snakeRect = new Rectangle(_rect1.Location.X, _rect1.Location.Y, _rect1.Width, _rect1.Height);
            var isCollision = snakeRect.Contains(_appleRect.Location.X, _appleRect.Location.Y);

            if (isCollision)
            {
                PacketData.ApplePos pos = new PacketData.ApplePos("1");
                _client.TCPSend(pos);
                PacketData.Score scorePacket = new PacketData.Score("1.1");
                _client.TCPSend(scorePacket);
            }

            snakeRect = new Rectangle(_rect2.Location.X, _rect2.Location.Y, _rect2.Width, _rect2.Height);
            isCollision = snakeRect.Contains(_appleRect.Location.X, _appleRect.Location.Y);

            if (isCollision)
            {
                PacketData.ApplePos pos = new PacketData.ApplePos("1");
                _client.TCPSend(pos);
                PacketData.Score scorePacket = new PacketData.Score("1.2");
                _client.TCPSend(scorePacket);
            }

            Invalidate();
        }

        private void SnakeGame_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.A)
            {
                PacketData.GameMovement packet = new PacketData.GameMovement("A");
                _client.TCPSend(packet);
            }
            else if (e.KeyCode == Keys.D)
            {
                PacketData.GameMovement packet = new PacketData.GameMovement("D");
                _client.TCPSend(packet);
            }
            else if (e.KeyCode == Keys.W)
            {
                PacketData.GameMovement packet = new PacketData.GameMovement("W");
                _client.TCPSend(packet);
            }
            else if (e.KeyCode == Keys.S)
            {
                PacketData.GameMovement packet = new PacketData.GameMovement("S");
                _client.TCPSend(packet);
            }
            else if(e.KeyCode == Keys.Space)
            {
                PacketData.GameMovement packet = new PacketData.GameMovement("P");
                _client.TCPSend(packet);
            }
        }

        
    }
}
