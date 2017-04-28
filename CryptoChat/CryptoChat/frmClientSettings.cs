/*
*   FILE            : frmClientSettings.cs
*   PROJECT         : ACS Assignment 2
*   PROGRAMMER      : Jackson Ruby & Brad Carradine
*   FIRST VERSION   : 03/03/2016
*   DESCRIPTION     :
*       This file deals with the settings for the client form.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoChat
{
    public partial class frmClientSettings : Form
    {
        /*
        *   FUNCTION    : frmClientSettings()
        *   DESCRIPTION : Constructor.
        */
        public frmClientSettings()
        {
            InitializeComponent();
        }

        /*
        *   FUNCTION    : ip1_TextChanged()
        *   DESCRIPTION : Text entered to this textbox.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void ip1_TextChanged(object sender, EventArgs e)
        {
            //move to next textbox if 3 characters are entered
            if (ip1.Text.Length == 3)
            {
                this.ActiveControl = ip2;
            }
        }

        /*
        *   FUNCTION    : ip2_TextChanged()
        *   DESCRIPTION : Text entered to this textbox.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void ip2_TextChanged(object sender, EventArgs e)
        {
            //move to next textbox if 3 characters are entered
            if (ip2.Text.Length == 3)
            {
                this.ActiveControl = ip3;
            }
        }

        /*
        *   FUNCTION    : ip3_TextChanged()
        *   DESCRIPTION : Text entered to this textbox.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void ip3_TextChanged(object sender, EventArgs e)
        {
            //move to next textbox if 3 characters are entered
            if (ip3.Text.Length == 3)
            {
                this.ActiveControl = ip4;
            }
        }

        /*
        *   FUNCTION    : ip4_TextChanged()
        *   DESCRIPTION : Text entered to this textbox.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void ip4_TextChanged(object sender, EventArgs e)
        {
            //move to next textbox if 3 characters are entered
            if (ip4.Text.Length == 3)
            {
                this.ActiveControl = txtPort;
            }
        }

        /*
        *   FUNCTION    : txtUsername_KeyPress()
        *   DESCRIPTION : Key pressed within this textbox.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            //check if character is enter and save settings if so
            if (e.KeyChar == 13)
            {
                SaveSettings();
            }
        }

        /*
        *   FUNCTION    : btnSave_Click()
        *   DESCRIPTION : Save button clicked.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void btnSave_Click(object sender, EventArgs e)
        {
            //save the settings
            SaveSettings();
        }

        /*
        *   FUNCTION    : SaveSettings()
        *   DESCRIPTION : Saves the settings.
        */
        private void SaveSettings()
        {
            //get the ipaddress and port from the textboxes
            string tempIP = ip1.Text + "." + ip2.Text + "." + ip3.Text + "." + ip4.Text;
            int tempPort = 0;
            IPAddress IP;
            
            //ensure the ipaddress is valid
            if (IPAddress.TryParse(tempIP, out IP))
            {
                //esnure the port is valid
                if (int.TryParse(txtPort.Text, out tempPort))
                {
                    //ensure the username is valid
                    if (!string.IsNullOrEmpty(txtUsername.Text))
                    {
                        //save the settings
                        frmClient.ServerIP = IP;
                        frmClient.port = tempPort;
                        frmClient.username = txtUsername.Text;
                        this.Hide();
                    }
                    else
                    {
                        StatusInfo.Text = "Invalid Username.";
                    }                    
                }
                else
                {
                    StatusInfo.Text = "Invalid Port.";
                } 
            }
            else
            {
                StatusInfo.Text = "Invalid IP Address.";
            } 
        }

        /*
        *   FUNCTION    : frmClientSettings_Load()
        *   DESCRIPTION : Upon form load.
        *   PARAMETERS  :
        *       object sender
        *       EventArgs e
        */
        private void frmClientSettings_Load(object sender, EventArgs e)
        {
            //if there was IP data already entered, replace it
            if (frmClient.ServerIP != null)
            {
                string ip = frmClient.ServerIP.ToString();
                string[] ipParts = ip.Split('.');
                ip1.Text = ipParts[0];
                ip2.Text = ipParts[1];
                ip3.Text = ipParts[2];
                ip4.Text = ipParts[3];
            }

            //if there was port data already entered, replace it
            if (frmClient.port != 0)
            {
                txtPort.Text = frmClient.port.ToString();
            }
            else
            {
                //otherwise set to default
                txtPort.Text = "3333";
            }

            //if there was username data already entered, replace it
            if (!string.IsNullOrEmpty(frmClient.username))
            {
                txtUsername.Text = frmClient.username;
            }
        }
    }
}
