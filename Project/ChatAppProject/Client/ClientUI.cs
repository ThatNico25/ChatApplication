using SimpleTCP;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Client
{
    /// <summary>
    /// This class is responsible for the UI of the client and its comportement. 
    /// </summary>
    public partial class ClientUI : Form
    {
        /// <summary>
        /// This is the object resposible to send/receive Data from the server.
        /// </summary>
        private SimpleTcpClient client;
        /// <summary>
        /// Check if the server is connected.
        /// - True : The connection is workings.
        /// - False : The connection fail or is currently not connected.
        /// </summary>
        private bool m_IsServerConnected = false;
        /// <summary>
        /// Restriction for what chracter the user can put in the name.
        /// </summary>
        private Regex usernameLimit = new Regex("^[A-Za-z0-9_.]+$");

        /// <summary>
        /// Create and Initialize the UI of the client interface.
        /// </summary>
        public ClientUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Connect to the server associated with the specified IP and port.
        /// </summary>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!usernameLimit.Match(txtUsername.Text).Success)
            {
                txtUsername.Text = "You can only use [aA-zZ0-9} characters.";
            }
            else
            {
                client = new SimpleTcpClient();
                try
                {
                    client.Connect(txtHost.Text, Convert.ToInt32(txtPort.Text));
                }
                catch (Exception ex)
                {
                    txtStatus.Clear();
                    txtStatus.Text += "Error : Can't connect to the server!~";
                }
                finally
                {
                    if(client.TcpClient.Connected)
                    {
                        txtStatus.Clear();
                        btnConnect.Enabled = false;
                        txtUsername.Enabled = false;
                        txtHost.Enabled = false;
                        txtPort.Enabled = false;
                        btnDisconnect.Enabled = true;
                        client.StringEncoder = Encoding.UTF8;
                        client.DataReceived += Client_DataReceived;
                        m_IsServerConnected = client.TcpClient.Connected;
                        client.WriteLineAndGetReply(txtUsername.Text.Substring(0, txtUsername.Text.Length), TimeSpan.FromSeconds(1));
                    }
                }
            }
        }

        /// <summary>
        /// Setup the ClientUI the moment it open.
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            btnDisconnect.Enabled = false;
        }

        /// <summary>
        /// Every data send from the server is being collected here. This is also where we tell to the program what to do with those informations.
        /// </summary>
        private void Client_DataReceived(object sender, SimpleTCP.Message e)
        {
            if(m_IsServerConnected)
            {
                String[] list = e.MessageString.ToString().Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                txtListUsername.Invoke((MethodInvoker) delegate()
                {
                    if (!list[0].ToString().Any(Char.IsWhiteSpace) && list[0].ToString() != "")
                    {
                        txtListUsername.Clear();
                    }

                    for (int i = 0; i < list.Length; i++)
                    {
                        if (!list[i].ToString().Any(Char.IsWhiteSpace) && list[i].ToString() != "")
                        {
                            String message = e.MessageString.Substring(0, e.MessageString.Length - 1);
                            txtListUsername.Text += list[i] + System.Environment.NewLine;
                            list[i] = "";
                        }
                    }
                });

                txtStatus.Invoke((MethodInvoker) delegate()
                {
                    String message = e.MessageString.Substring(0, e.MessageString.Length - 1);

                    if (txtStatus.Text.Length == 0)
                    {
                        if (m_IsServerConnected && list[list.Length - 2].Any(Char.IsWhiteSpace))
                        {
                            txtStatus.Text += list[list.Length - 2];
                        }
                    }
                    else
                    {
                        if (m_IsServerConnected && list[list.Length - 2].Any(Char.IsWhiteSpace))
                        {
                            txtStatus.Text += System.Environment.NewLine + list[list.Length - 2];
                        }
                    }        
                    if (message.Contains("You got disconnected because the server is closed!"))
                    {
                        Disconnect();
                    }
                });
            }
        }

        /// <summary>
        /// Send the message that the user wrote to the server.
        /// </summary>
        private void btnSend_Click(object sender, EventArgs e)
        {
            if(m_IsServerConnected)
            {
                client.WriteLineAndGetReply(txtUsername.Text + " : " + txtMessage.Text, TimeSpan.FromSeconds(0));
                txtMessage.Clear();
            }
        }

        /// <summary>
        /// Disconnect from the current server connected.
        /// </summary>
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            client.WriteLineAndGetReply(txtUsername.Text.Substring(0, txtUsername.Text.Length), TimeSpan.FromSeconds(1));
            txtHost.Enabled = true;
            txtPort.Enabled = true;
            txtUsername.Enabled = true;
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            m_IsServerConnected = false;
            txtListUsername.Clear();
            txtStatus.Text += System.Environment.NewLine + txtUsername.Text + " log off...";
            client.Disconnect();
        }
    }
}