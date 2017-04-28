/*
*   FILE            : frmClient.cs
*   PROJECT         : ACS Assignment 2
*   PROGRAMMER      : Jackson Ruby & Brad Carradine
*   FIRST VERSION   : 03/03/2016
*   DESCRIPTION     :
*       This file deals with all the client functionality.
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoChat
{
    public partial class frmClient : Form
    {
        /*-- Client settings --*/
        public static string username = "";
        public static int port = 0;
        public static IPAddress ServerIP;
        private int ID = 0;

        /*-- Network settings --*/
        private IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        private IPEndPoint ipEndPoint;
        private Socket server;
        private NetworkStream theNetworkStream;

        /*-- Thread settings --*/
        private Thread ReceiveThread;
        private bool doReceive = false;

        /*
        *   FUNCTION    : frmClient()
        *   DESCRIPTION : Constructor.
        */
        public frmClient()
        {
            InitializeComponent();
        }

        /*
        *   FUNCTION    : frmMain_Load()
        *   DESCRIPTION : Set up form upon load.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void frmMain_Load(object sender, EventArgs e)
        {
            //set up the form
            btnEncrypt.BackgroundImage = Image.FromFile("locked.png");
            btnSettings.BackgroundImage = Image.FromFile("settings.png");
            Encryption.encryptText = true;
            this.ActiveControl = txtMessage;
        }

        /*
        *   FUNCTION    : btnEncrypt_Click()
        *   DESCRIPTION : Turn encryption on or off.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            //set encryption on or off
            if (Encryption.encryptText)
            {
                btnEncrypt.BackgroundImage = Image.FromFile("unlocked.png");
                Encryption.encryptText = false;
            }
            else
            {
                btnEncrypt.BackgroundImage = Image.FromFile("locked.png");
                Encryption.encryptText = true;
            }
        }

        /*
        *   FUNCTION    : btnSend_Click()
        *   DESCRIPTION : Send a message.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void btnSend_Click(object sender, EventArgs e)
        {
            //ensure the message isnt empty
            if (!string.IsNullOrEmpty(txtMessage.Text))
            {
                //ensure the server is connected
                if (server == null || !server.Connected)
                {
                    //ensure the message isnt a command
                    if (!checkCommands(txtMessage.Text))
                    {
                        txtChat.Text += "Use /help for help." + Environment.NewLine;
                        txtMessage.Clear();
                    }
                }
                else if (!checkCommands(txtMessage.Text))
                {
                    //send the message
                    send(txtMessage.Text);
                }
            }
        }

        /*
        *   FUNCTION    : checkCommands()
        *   DESCRIPTION : Checks if a message is a command.
        *   PARAMETERS  :
        *       string text
        *   RETURNS     :
        *       bool
        */
        private bool checkCommands(string text)
        {
            bool commandFound = false;

            //check if command
            if (text.ToLower() == "/connect")
            {
                //ensure settings are set before conencting
                if (ServerIP != null &&
                    port != 0 &&
                    !string.IsNullOrEmpty(username))
                {
                    connect();
                }
                else
                {
                    txtChat.Text += "Invalid connection parameters, check settings." + Environment.NewLine;
                }
                commandFound = true;
                txtMessage.Clear();
            }
            else if (text.ToLower() == "/clear")
            {
                //clear the chat
                txtChat.Clear();
                commandFound = true;
                txtMessage.Clear();
            }
            else if (text.ToLower() == "/disconnect" && (server != null && server.Connected))
            {
                //disconnect from the server
                server.Send(Encoding.ASCII.GetBytes("disconnect"));
                AddText("Disconnected from server.");
                server.Shutdown(SocketShutdown.Both);
                server.Close();
                theNetworkStream.Close();
                commandFound = true;
                txtMessage.Clear();
            }
            else if (text.ToLower() == "/disconnect" && (server == null || !server.Connected))
            {
                //let the user know they havent even connected yet
                txtChat.Text += "Client not connected." + Environment.NewLine;
                commandFound = true;
                txtMessage.Clear();
            }
            else if (text.ToLower() == "/help")
            {
                //show help
                txtChat.Text += "/connect\t\t- Connect to the server." + Environment.NewLine;
                txtChat.Text += "/disconnect\t- Disconnect from the server." + Environment.NewLine;
                txtChat.Text += "/clear\t\t- Clear the chat." + Environment.NewLine;
                commandFound = true;
                txtMessage.Clear();
            }
            return commandFound;
        }

        /*
        *   FUNCTION    : AddText()
        *   DESCRIPTION : Adds text to the chat.
        *   PARAMETERS  :
        *       string text
        */
        public void AddText(string text)
        {
            //if the function is called from a thread, invoke a new action to display the text
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AddText), new object[] { text });
                return;
            }
            txtChat.Text += text + Environment.NewLine;
            txtChat.SelectionStart = txtChat.Text.Length;
            txtChat.ScrollToCaret();
        }

        /*
        *   FUNCTION    : frmClient_FormClosing()
        *   DESCRIPTION : Upon the closing of the form.
        *   PARAMETERS  :
        *       object sender
        *       FormClosingEventArgs e
        */
        private void frmClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            //shutdown
            try
            {
                server.Shutdown(SocketShutdown.Both);
                server.Close();
            }
            catch { }
            Application.Exit();
        }

        /*
        *   FUNCTION    : txtMessage_KeyDown()
        *   DESCRIPTION : Check for enter key in the message field.
        *   PARAMETERS  :
        *       object sender
        *       KeyEventArgs e
        */
        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            //press send if enter is pressed
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(sender, new EventArgs());
            }
        }

        /*
        *   FUNCTION    : btnSettings_Click()
        *   DESCRIPTION : Go to the settings form.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void btnSettings_Click(object sender, EventArgs e)
        {
            //open the settings form.
            frmClientSettings cSettings = new frmClientSettings();
            cSettings.Location = this.Location;
            cSettings.Show();
        }

        /*
        *   FUNCTION    : connect()
        *   DESCRIPTION : Connect to the server.
        *   RETURNS     :
        *       bool
        */
        private bool connect()
        {
            bool allOk = true;

            //set up the connection
            AddText("Attempting to connect.");
            ipEndPoint = new IPEndPoint(ServerIP, port);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.SendTimeout = 1000;
            server.ReceiveTimeout = 1000;

            try
            {
                //connect
                server.Connect(ipEndPoint);
                theNetworkStream = new NetworkStream(server);
                AddText("Connected Successfully.");
                
                //get id number from the server
                string IDmsg = "";
                byte[] bytes = new byte[10024];
                int numBytes = 0;
                while (IDmsg == "")
                {
                    if (server.Poll(1000, SelectMode.SelectRead))
                    {
                        numBytes = server.Receive(bytes);
                        IDmsg = Encoding.ASCII.GetString(bytes, 0, numBytes);
                        IDmsg = IDmsg.Trim('\0');
                    }
                }

                //split message to find id number
                string[] splitMsg = IDmsg.Split('=');
                if (splitMsg.Length == 2 && splitMsg[0] == "ID")
                {
                    ID = int.Parse(splitMsg[1]);
                }

                //start receive thread
                doReceive = true;
                ReceiveThread = new Thread(new ThreadStart(rcv));
                ReceiveThread.Start();
            }
            catch
            {
                allOk = false;
            }

            return allOk;
        }

        /*
        *   FUNCTION    : send()
        *   DESCRIPTION : Send a message.
        *   PARAMETERS  :
        *       string text
        */
        private void send(string text)
        {
            //set up the json message
            JSONMessage msg = new JSONMessage();

            //set the message depending on encryption toggle
            if (Encryption.encryptText)
            {
                msg.Encrypted = true;
                msg.Message = Encryption.enc(txtMessage.Text);
            }
            else
            {
                msg.Encrypted = false;
                msg.Message = txtMessage.Text;
            }
            msg.SendingID = ID;
            msg.SendingName = username;

            //convert json to string
            string message = JsonConvert.SerializeObject(msg);
            try
            {
                //send message
                server.Send(Encoding.ASCII.GetBytes(message));
                txtMessage.Clear();
            }
            catch (Exception e)
            {
                AddText("An error occured: " + e.Message);
            }
        }

        /*
        *   FUNCTION    : rcv()
        *   DESCRIPTION : Thread function for receiving from the server.
        */
        private void rcv()
        {
            try
            {
                //receive until told not to
                while (doReceive == true)
                {
                    string data = "";
                    byte[] inputBuffer = new byte[10024];
                    int numBytes = 0;

                    //receive from the server
                    while (data == "")
                    {
                        if (server.Poll(100, SelectMode.SelectRead))
                        {
                            numBytes = server.Receive(inputBuffer);
                            data = Encoding.ASCII.GetString(inputBuffer, 0, numBytes);
                            data = data.Trim('\0');
                        }

                    }

                    if (data != "close")
                    {
                        //convert to json message
                        JSONMessage JSONMsg = JsonConvert.DeserializeObject<JSONMessage>(data);
                        if (JSONMsg != null)
                        {
                            string name = JSONMsg.SendingName;
                            string msg = JSONMsg.Message;

                            //display message
                            AddText("[" + name + "]: " + msg);
                            msg = Encryption.dec(msg);
                            AddText("[" + name + "][Encrypted]: " + msg);
                        }
                    }
                    else
                    {
                        //shutdown message received, so shutdown, duh
                        AddText("Server has shutdown.");
                        doReceive = false;
                        server.Close();
                        theNetworkStream.Close();
                    }
                }
            }
            catch { }
        }
    }
}