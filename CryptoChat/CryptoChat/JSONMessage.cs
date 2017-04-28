using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoChat
{
    class JSONMessage
    {
        private int sendingID;
        private string sendingName;
        private bool encrypted;
        private string message;

        public int SendingID
        {
            get { return sendingID; }
            set { sendingID = value; }
        }

        public string SendingName
        {
            get { return sendingName; }
            set { sendingName = value; }
        }

        public bool Encrypted
        {
            get { return encrypted; }
            set { encrypted = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
