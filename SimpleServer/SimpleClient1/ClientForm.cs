using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleClient
{
    public partial class ClientForm : Form
    {
        delegate void UpdateChatWindowDelegate(string message);
        UpdateChatWindowDelegate _updateChatWindowDelgate;
        SimpleClient _client;

        public ClientForm(SimpleClient client)
        {
            _client = client;
            InitializeComponent();
            _updateChatWindowDelgate = new UpdateChatWindowDelegate(UpdateChatWindow);
           
            ControlSelect(SubmitText);
        }

        public void ControlSelect(Control control)
        {
            // Select the control, if it can be selected.
                if (control.CanSelect)
                {
                    control.Select();
                }
        }

        public void UpdateChatWindow(String message)
        {
                if (OutText.InvokeRequired)
                {
                    Invoke(_updateChatWindowDelgate, message);
                }
                else
                {
                    OutText.Text += message + "\n";
                    OutText.SelectionStart = OutText.Text.Length;
                    OutText.ScrollToCaret();
                }
        }

        public void Quit()
        {

        }

        //input text box
        private void richTextBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        //displayed text
        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
 
        }

        //submit text button
        private void SubmitButton_Click(object sender, EventArgs e)
        {
            string input = SubmitText.Text;

            PacketData.ChatMessagePacket packet = new PacketData.ChatMessagePacket(SubmitText.Text);
            _client.TCPSend(packet);
            SubmitText.Clear();
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            _client.Readerthread.Start();
        }

        private void ClientForm_Closed(object sender, EventArgs e)
        {
            _client.TCPStop();
        }

        //quit button
        private void button1_Click(object sender, EventArgs e)
        {
            PacketData.QuitPacket packet = new PacketData.QuitPacket();
            _client.TCPSend(packet);
        }

        //nickName text box
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string input = nickName.Text;
            
            PacketData.NickNamePakcet packet = new PacketData.NickNamePakcet(input);
            _client.TCPSend(packet);
            SubmitText.Clear();     
        }
    }
}
