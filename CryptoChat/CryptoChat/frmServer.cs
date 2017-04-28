/*
*   FILE            : frmServer.cs
*   PROJECT         : ACS Assignment 2
*   PROGRAMMER      : Jackson Ruby & Brad Carradine
*   FIRST VERSION   : 03/03/2016
*   DESCRIPTION     :
*       This file deals with all the server functionality.
*/

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CryptoChat
{
    public partial class frmServer : Form
    {
        //thread setup
        private Thread mainReceiveThread;
        public static bool keepReceiving = true;

        //queue for messages
        private ConcurrentQueue<JSONMessage> MessageQueue = new ConcurrentQueue<JSONMessage>();

        //list of threads and clients
        private List<Thread> threadList = new List<Thread>();
        private List<ClientThread> clientList = new List<ClientThread>();

        /*
        *   FUNCTION    : frmServer()
        *   DESCRIPTION : Constructor.
        */
        public frmServer()
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
            //start the thread for receiving new clients
            mainReceiveThread = new Thread(new ThreadStart(getClients));
            mainReceiveThread.Name = "Main Receive Thread";
            mainReceiveThread.Start();
        }

        /*
        *   FUNCTION    : getClients()
        *   DESCRIPTION : Thread for receiving new clients.
        */
        public void getClients()
        {
            //start server
            AddText("Server set up complete. Waiting on clients.");
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 3333);
            serverSocket.Start();
            try
            {
                //continue to receive until told not to
                while (keepReceiving)
                {
                    //wait for a client to be pending
                    if (serverSocket.Pending())
                    {
                        //set up new client
                        int ClientID = clientList.Count + 1;
                        ClientThread newClient = new ClientThread(ClientID, default(TcpClient));
                        newClient.ClientConnection = serverSocket.AcceptTcpClient();
                        AddText("New Client [ID#" + ClientID + "] Connected.");

                        //send client its ID number
                        byte[] bytes = new Byte[10024];
                        string data = "ID=" + ClientID;
                        NetworkStream ns = newClient.ClientConnection.GetStream();
                        bytes = Encoding.ASCII.GetBytes(data);
                        ns.Write(bytes, 0, bytes.Length);
                        ns.Flush();

                        //add client to list and start thread for it
                        clientList.Add(newClient);
                        Thread newThread = new Thread(new ParameterizedThreadStart(recv));
                        newThread.Name = "Client Thread #" + ClientID.ToString();
                        newThread.Start(newClient);
                    }
                   
                }
            }
            catch { }
        }

        /*
        *   FUNCTION    : btnClose_Click()
        *   DESCRIPTION : Close the server.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void btnClose_Click(object sender, EventArgs e)
        {
            frmServer_FormClosing(sender, new FormClosingEventArgs(CloseReason.ApplicationExitCall, false));
        }

        /*
        *   FUNCTION    : AddText()
        *   DESCRIPTION : Adds text to the chat.
        *   PARAMETERS  :
        *       string text
        */
        public void AddText(string text)
        {
            try
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
            catch { }
        }

        /*
        *   FUNCTION    : frmServer_FormClosing()
        *   DESCRIPTION : Form is closing()
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void frmServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            keepReceiving = false;
            mainReceiveThread.Join();

            //close each client
            foreach (ClientThread ct in clientList)
            {
                NetworkStream ns = ct.ClientConnection.GetStream();
                byte[] bytes = new byte[10024];
                bytes = Encoding.ASCII.GetBytes("close");
                ns.Write(bytes, 0, bytes.Length);
                ns.Flush();
            }
            foreach (Thread CT in threadList)
            {
                CT.Join();
            }
            Application.Exit();
        }

        /*
        *   FUNCTION    : recv()
        *   DESCRIPTION : Do receive for a given client.
        *   PARAMETERS  :
        *       object clientObj
        */
        private void recv(object clientObj)
        {
            //get the client
            ClientThread client = (ClientThread)clientObj;
             
            try
            {
                //keep receiving until told not to
                while (keepReceiving)
                {
                    byte[] bytes = new Byte[10024];
                    string data = "";

                    //ensure connection is still established
                    if (client.ClientConnection.Connected)
                    {
                        //read in a message
                        NetworkStream ns = client.ClientConnection.GetStream();
                        ns.Read(bytes, 0, bytes.Length);
                        data = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                        data = data.Trim('\0');

                        if (data == "disconnect")
                        {
                            //if the message is diconnect, then disconnect, duh
                            AddText("[ID#" + client.ID + "] Disconnected.");
                            client.ClientConnection.Close();
                            clientList.Remove(client);
                        }
                        else
                        {
                            //if the message is a message, then convert it to a json object
                            JSONMessage theMessage = JsonConvert.DeserializeObject<JSONMessage>(data);
                            AddText("[ID#" + client.ID + "][" + theMessage.SendingName + "] Message Received:");
                            if (!theMessage.Encrypted)
                            {
                                AddText("Message: " + theMessage.Message + Environment.NewLine);
                            }
                            else
                            {
                                AddText("Encrypted: " + theMessage.Message);
                                AddText("Decrypted: " + Encryption.dec(theMessage.Message) + Environment.NewLine);
                            }

                            //enqueue the message
                            MessageQueue.Enqueue(theMessage);

                            //start the send thread
                            Thread sendThread = new Thread(new ThreadStart(send));
                            sendThread.Name = "Send Thread";
                            sendThread.Start();
                        }
                        
                    }

                }
            }
            catch (Exception e)
            {
                AddText("An error occured: " + e.Message);
            }
           
        }

        /*
        *   FUNCTION    : send()
        *   DESCRIPTION : Send what's in the queue.
        */
        private void send()
        {
            //set up data for sending
            JSONMessage temp;
            string json = "";
            byte[] bytes = new byte[10024];

            //ensure theres a message available to be sent
            if (MessageQueue.TryDequeue(out temp))
            {
                json = JsonConvert.SerializeObject(temp);
            }

            //send the message to each client
            foreach (ClientThread ct in clientList)
            {
                NetworkStream ns = ct.ClientConnection.GetStream();
                bytes = Encoding.ASCII.GetBytes(json);
                ns.Write(bytes, 0, bytes.Length);
                ns.Flush();
            }
        }
    }
}