using SimpleTCP;
using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChatAppProject
{
    /// <summary>
    /// This class is responsible for the UI of the server and its comportement. 
    /// </summary>
    public partial class ServerUI : Form
    {
        /// <summary>
        /// This is the object resposible to send/receive Data from the client.
        /// </summary>
        SimpleTcpServer server;
        /// <summary>
        /// This variables check if a client recently connected to the server.
        /// </summary>
        private bool m_isUserLoggingIn = false;
        /// <summary>
        /// This variables check if a client recently disconnected to the server.
        /// </summary>
        private bool m_isUserDisconnect = false;

        /// <summary>
        /// Create and Initialize the UI of the server interface.
        /// </summary>
        public ServerUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Setup the ServerUI the moment it open.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            server = new SimpleTcpServer();
            server.Delimiter = 0x13;
            server.StringEncoder = Encoding.UTF8;
            server.DataReceived += Server_DataReceived;
        }

        /// <summary>
        ///  Every data send from the client is being collected here. This is also where we tell to the program what to do with those informations.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            txtListUsername.Invoke((MethodInvoker) delegate()
            {
                String message = e.MessageString.Substring(0, e.MessageString.Length - 1);
                m_isUserDisconnect = txtListUsername.Text.ToString().Contains(message) && !message.ToString().Any(Char.IsWhiteSpace);

                if (!txtListUsername.Text.ToString().Contains(message) && !message.ToString().Any(Char.IsWhiteSpace))
                {
                    txtListUsername.Text += message + System.Environment.NewLine;
                    server.Broadcast(txtListUsername.Text.ToString() + System.Environment.NewLine); 
                    m_isUserLoggingIn = true;
                }
            });

            txtStatus.Invoke((MethodInvoker) delegate()
            {
                if (m_isUserLoggingIn)
                {
                    String message = e.MessageString.Substring(0, e.MessageString.Length - 1);
                    txtStatus.Text += message + " is connected to the server." + System.Environment.NewLine;
                    server.BroadcastLine(message + " is connected to the server." + System.Environment.NewLine);
                    m_isUserLoggingIn = false;
                }
                else if(m_isUserDisconnect)
                {
                    String message = e.MessageString.Substring(0, e.MessageString.Length - 1);
                    txtStatus.Text += message + " is disconnected to the server." + System.Environment.NewLine;
                    m_isUserDisconnect = false;
                    String remover = message + System.Environment.NewLine;
                    String temp = txtListUsername.Text;
                    txtListUsername.Clear();
                    int start = temp.IndexOf(remover);
                    String temp2 = temp.Remove(start, remover.Length);
                    txtListUsername.Text = temp2;
                    server.Broadcast(txtListUsername.Text.ToString() + System.Environment.NewLine);
                    server.BroadcastLine(message + " is disconnected to the server." + System.Environment.NewLine);
                }
                else
                {
                    String message = e.MessageString.Substring(0, e.MessageString.Length - 1);
                    txtStatus.Text += message + System.Environment.NewLine;
                    server.BroadcastLine(message + System.Environment.NewLine);
                }
            });
        }

        /// <summary>
        /// Start the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            txtStatus.Text += "Server Starting..." + System.Environment.NewLine;
            server.Start(IPAddress.Parse(txtHost.Text), Convert.ToInt32(txtPort.Text));
            btnStart.Enabled = false;
            btnDisconnect.Enabled = true;
            txtHost.Enabled = false;
            txtPort.Enabled = false;
            txtStatus.Text += "Server is now open!..." + System.Environment.NewLine;
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if(server.IsStarted)
            {
                txtStatus.Text += "Server is stopping..." + System.Environment.NewLine;
                btnStart.Enabled = true;
                btnDisconnect.Enabled = false;
                txtHost.Enabled = true;
                txtPort.Enabled = true;
                txtListUsername.Text = "";
                server.Stop();
                txtStatus.Text += "Server is now close!" + System.Environment.NewLine;
            }
        }
    }
}