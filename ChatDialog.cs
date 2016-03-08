/*
 * Project: Yellow Taxi
 * File: ChatDialog.cs
 * Programmer: Matthew Thiessen, Frank Taylor, Jordan Poirier, Tylor McLaughlin
 * First Version: Nov.11/2015
 * Description: This file contains a chat dialog box which represents
 *              a thread used to talk to a specific client.
 * Reference: This project is based on a chat program example found
 *            on http://www.codeproject.com/Articles/16023/Multithreaded-Chat-Server
 */

using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Data.SQLite;

namespace Atlas
{
    /*
     * Class: ChatDialog
     * Description: A chat dialog box used to communicate with
     *              a specific client.
     */
    public partial class ChatDialog : Form
    {
        private TcpClient client;
        private NetworkStream clientStream;
        private AtlasComm atlasComm;
        public delegate void SetTextCallback(string s);
        private atlas owner;
        private SQLiteConnection connection; //connect 
        private int xClientPos;
        private int yClientPos;
        public int pBodyCount = 0;
        public int pBodyId = 0;
        private int leftBoundry = 0;
        private int rightBoundry = 0;
        private int upperBoundry = 0;
        private int lowerBoundry = 0;
        private int lBound = 0;
        private int rBound = 0;
        private int uBound = 0;
        private int loBound = 0;
        private int startingXValue = 0;
        private int startingYValue = 0;
        private int restXPos = 0;
        private int restYPos = 0;
        public bool stopJob = true;
        private bool startPos = true;
        private bool sweepRight = true;
        private bool returnRestPos = false;

        /*
         * Method: ChatDialog()
         * Parameter: nothing
         * Return: nothing
         * Description: initializes the chat dialog box
         */
        public ChatDialog()
        {
            InitializeComponent();
        }

        /*
         * Method: ChatDialog()
         * Parameter: atlas, TcpClient, int, int, int, int, int, int
         * Return: nothing
         * Description: initializes the chat dialog box with the
         *              parent and target client as arguments
         */
        public ChatDialog(atlas parent, TcpClient tcpClient, int leftBound, int rightBound, int upperBound, int lowerBound, int xPos, int yPos, int pBdyId)
        {
            InitializeComponent();

            pBodyId = pBdyId;
            pBodyCount = pBdyId + 1;
            xClientPos = xPos;
            yClientPos = yPos;
            restXPos = xPos;
            restYPos = yPos;
            lBound = leftBound;
            rBound = rightBound;
            uBound = upperBound;
            loBound = lowerBound;

            this.owner = parent;
            // Get Stream Object
            connectedClient = tcpClient;
            clientStream = tcpClient.GetStream();
            atlasComm = new AtlasComm(tcpClient);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = connectedClient.Client;

            //Call Asynchronous Receive Function
            connectedClient.Client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(OnReceive), state);

            connection = new SQLiteConnection("Data Source=C:\\SQLite\\WorldMap.db; Version = 3;");

            atlasComm.AssignNewKey();
            atlasComm.SendPublicKey();
            Thread.Sleep(1000);
            SendStartingLocation();
        }

        public TcpClient connectedClient
        {
            get { return client; } //gets the current client
            set { client = value; } //sets the current client
        }

        /*
         * Method: SetText()
         * Parameter: string
         * Return: void
         * Description: Sets the text received from the client to
         *              the text boc in the chat dialog box.
         */
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.rtbChat.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.rtbChat.SelectionColor = Color.Blue;
                this.rtbChat.SelectedText = "\nP-Body " + pBodyId.ToString() + " / " + pBodyCount.ToString() + ": " + text;
            }
        }

        /*
         * Method: SendStartingLocation()
         * Parameters: nothing
         * Return: void
         * Description: Send the client its starting position.
         *              This is where it will begin its job.
         */
        private void SendStartingLocation()
        {
            byte[] bt;
            bt = Encoding.ASCII.GetBytes(/*atlasComm.EncryptData(*/"STT," + xClientPos + "," + yClientPos/*)*/);
            connectedClient.Client.Send(bt);
        }
        
        /*
         * Method: btnSend_Click()
         * Parameter: object, EventArgs
         * Return: void
         * Description: sends the desired message to the client
         *              and displays it to the chat dialog box
         */
        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] bt;
            bt = Encoding.ASCII.GetBytes(/*atlasComm.EncryptData(*/txtMessage.Text/*)*/);
            atlasComm.connectedClient.Client.Send(bt);

            rtbChat.SelectionColor = Color.IndianRed;
            rtbChat.SelectedText = "\nMe:     " + txtMessage.Text;
            txtMessage.Text = "";
        }

        /*
         * Method: OnReceive()
         * Parameter: IAsyncResult
         * Return: void
         * Description: Upon receiving data(message) from the
         *              client, display it to the chat dialog box.
         */
        public void OnReceive(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead;

            if (handler.Connected)
            {
                // Read data from the client socket. 
                try
                {
                    bytesRead = handler.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        // There  might be more data, so store the data received so far.
                        state.sb.Remove(0, state.sb.Length);
                        state.sb.Append(Encoding.ASCII.GetString(
                                         state.buffer, 0, bytesRead));

                        content = state.sb.ToString();

                        //checks data for public key from client
                        if (content.Contains("RSAKeyValue"))
                        {
                            atlasComm.publicKey = content;
                        }
                        else
                        {
                            if (!stopJob)
                            {
                                //content = atlasComm.DecryptData(content);

                                // Display Text in Rich Text Box
                                SetText(content);
                                ProcessData(content);
                            }
                        }

                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(OnReceive), state);

                    }
                }

                catch (SocketException socketException)
                {
                    //client closed suddenly
                    if (socketException.ErrorCode == 10054 || ((socketException.ErrorCode != 10004) && (socketException.ErrorCode != 10053)))
                    {
                        // Complete the disconnect request.
                        String remoteIP = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
                        String remotePort = ((IPEndPoint)handler.RemoteEndPoint).Port.ToString();
                        this.owner.DisconnectClient(remoteIP, remotePort);

                        handler.Close();
                        handler = null;

                    }
                }

                //display exception
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message + "\n" + exception.StackTrace);
                }
            }
        }

        /*
         * Method: ChatDialog_FormClosing()
         * Parameter: Object, FormClosingEventArgs
         * Return: void
         * Description: conceals the form when closing
         */
        private void ChatDialog_FormClosing(Object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        /*
         * Method: UpdateWorldMap()
         * Parameter: String
         * Return: void
         * Description: Will update the world map based on the
         *              client data
         */
        private void UpdateWorldMap(String data)
        {
            SQLiteCommand command;
            string[] dataProcess = new string[] { };

            //take info from client and format it for database storage
            dataProcess = data.Split(',');
            string query = "insert into WorldMap (Coordinate, IsObstructed) values ('" + dataProcess[0] + "," + dataProcess[1] + "', " + dataProcess[2] + ")";

            connection.Open();

            command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();

            connection.Close();
        }

        /*
         * Method: ProcessData()
         * Parameter: String
         * Return: void
         * Description: Will take in the data sent by the client and
         *              process it. It will be then be used to update the
         *              world map
         */
        private void ProcessData(String data)
        {
            string[] coordinates = new string[] { };
            int curXPos = 0;
            int curYPos = 0;
            int isObstructed = 0;
            string obstructionDirection = "";
            string travelDirection = "";
            coordinates = data.Split(',');

            Int32.TryParse(coordinates[1], out curXPos);
            Int32.TryParse(coordinates[2], out curYPos);
            Int32.TryParse(coordinates[4], out isObstructed);
            obstructionDirection = coordinates[6];
            travelDirection = coordinates[7];

            //if(isObstructed == 1)
            //{
            //    maneuvering = true;
            //    if(obstructionDirection == "F" || obstructionDirection == "B")
            //    {
            //        SendMessage("MOV,L");
            //    }
            //    else if (obstructionDirection == "R" || obstructionDirection == "L")
            //    {
            //        SendMessage("MOV,B");
            //    }
            //}
            //else if(maneuvering)
            //{

            //}
            //else if(isObstructed == 0 && obstructionDirection != "N")
            //{
            //    if(obstructionDirection == "B" || obstructionDirection == "F")
            //    {
            //        //if()
            //    }
            //}
            ////bring P_Body to starting point of the job
            //else
            if(returnRestPos)
            {
                if(curXPos > restXPos)
                {
                    atlasComm.SendMessage("MOV,L");
                }
                else if(curXPos < restXPos)
                {
                    atlasComm.SendMessage("MOV,R");
                }
                else if(curYPos < restYPos)
                {
                    atlasComm.SendMessage("MOV,F");
                }
                else if(curYPos > restYPos)
                {
                    atlasComm.SendMessage("MOV,B");
                }
                else
                {
                    atlasComm.SendMessage("MOV,S");
                    returnRestPos = false;
                    stopJob = true;
                }
            }
            else if (startPos)
            {
                if(curXPos > startingXValue)
                {
                    atlasComm.SendMessage("MOV,L");
                }
                else if(curYPos > startingYValue)
                {
                    atlasComm.SendMessage("MOV,B");
                }
                else if(curXPos < startingXValue)
                {
                    atlasComm.SendMessage("MOV,R");
                }
                else if(curYPos < startingYValue)
                {
                    atlasComm.SendMessage("MOV,F");
                }
                else
                {
                    atlasComm.SendMessage("MOV,S");
                    startPos = false;
                }
            }
            else
            {
                if (curYPos != upperBoundry)
                {
                    if (sweepRight)
                    {
                        if (curXPos == (rightBoundry - 1))
                        {
                            sweepRight = false;
                            atlasComm.SendMessage("MOV,F");
                        }
                        else
                        {
                            atlasComm.SendMessage("MOV,R");
                        }
                    }
                    else
                    {
                        if (curXPos == (leftBoundry))
                        {
                            sweepRight = true;
                            atlasComm.SendMessage("MOV,F");
                        }
                        else
                        {
                            atlasComm.SendMessage("MOV,L");
                        }
                    }
                }
                else
                {
                    atlasComm.SendMessage("MOV,S");
                }
                UpdateWorldMap(coordinates[1] + "," + coordinates[2] + "," + coordinates[4]);
            }
        }

        /*
         * Method: StartJob()
         * Parameter: nothing
         * Return: void
         * Description: Calculates the job bounds and start directing the
         *              P_Body through the job.
         */
        public void StartJob()
        {
            int width = rBound + lBound;
            int height = uBound + loBound;
            int mod = width % pBodyCount;
            int jobWidth = width / pBodyCount;

            leftBoundry = jobWidth * pBodyId - lBound;
            startingXValue = jobWidth * pBodyId - lBound;
            startingYValue = -loBound;
            upperBoundry = uBound;
            lowerBoundry = -loBound;

            if (pBodyId == (pBodyCount - 1))
            {
                rightBoundry = leftBoundry + jobWidth + mod;
            }
            else
            {
                rightBoundry = leftBoundry + jobWidth;
            }
            stopJob = false;
        }

        /*
         * Method: StopJob()
         * Parameter: nothing
         * Return: void
         * Description: Stops the current job and sends the robots back to their starting point
         */
        public void StopJob()
        {
            returnRestPos = true;
        }
    }
}