using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoChat
{
    public partial class frmStartup : Form
    {
        public frmStartup()
        {
            InitializeComponent();
        }

        private void btnClient_Click(object sender, EventArgs e)
        {
            frmClient client = new frmClient();
            client.Show();
            this.Hide();
        }

        private void btnServer_Click(object sender, EventArgs e)
        {
            frmServer server = new frmServer();
            server.Show();
            this.Hide();
        }
    }
}
