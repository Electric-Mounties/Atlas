/*
 * Project: CASAR Network
 * File: AtlasComm.cs
 * Programmer: Matthew Thiessen, Frank Taylor, Jordan Poirier, Tylor McLaughlin
 * First Version: Mar.1/2016
 * Description: This class is used to for sending and receiving
 *              messages as well as encrypting and decrypting them.
 *              It uses the RSA encryption class.
 */

using System;
using System.Text;
using System.Security.Cryptography;
using System.Net.Sockets;

namespace Atlas
{
    class AtlasComm
    {
        private string public_key = ""; //This is the public key
        private string public_key_client = ""; //This is the public key received from the client
        private string private_key = ""; //This is the private and public key.
        private RSACryptoServiceProvider rsa;
        private TcpClient client;

        AtlasComm()
        {

        }

        /*
         * Method: AtlasComm()
         * Parameters: TcpClient
         * Return: nothing
         * Description: AtlasComm constructor that takes in the
         *              target messaging client as a parameter.
         */
        public AtlasComm(TcpClient tcpClient)
        {
            connectedClient = tcpClient;
        }

        /*
         * Method: connectedClient()
         * Parameters: TcpClient
         * Return: TcpClient
         * Description: A getter/setter method for the client
         *              variable.
         */
        public TcpClient connectedClient
        {
            get { return client; } //gets the current client
            set { client = value; } //sets the current client
        }

        /*
         * Method: publicKey()
         * Parameters: string
         * Return: string
         * Description: A getter/setter method for the public_key_client
         *              variable.
         */
        public string publicKey
        {
            get { return public_key_client; } //gets the current public key
            set { public_key_client = value; } //sets the current public key
        }

        /*
         * Method: AssignNewKey()
         * Parameters: nothing
         * Return: void
         * Description: Upon initializing the communication thread,
         *              a unique private/public key pair is generated
         *              to be used for sequre message transfer
         */
        public void AssignNewKey()
        {
            const int PROVIDER_RSA_FULL = 1;
            const string CONTAINER_NAME = "KeyContainer";
            CspParameters cspParams;
            cspParams = new CspParameters(PROVIDER_RSA_FULL);
            cspParams.KeyContainerName = CONTAINER_NAME;
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParams.ProviderName = "Microsoft Strong Cryptographic Provider";
            rsa = new RSACryptoServiceProvider(cspParams);

            //Pair of public and private key as XML string.
            //Do not share this to other party
            private_key = rsa.ToXmlString(true);

            //Private key in xml file, this string should be share to other parties
            public_key = rsa.ToXmlString(false);
        }

        /*
         * Method: SendMessage()
         * Parameter: string
         * Return: void
         * Description: sends the desired message to the client
         *              and displays it to the chat dialog box
         */
        public void SendMessage(string message)
        {
            byte[] bt;
            bt = Encoding.ASCII.GetBytes(/*EncryptData(*/message/*)*/);
            connectedClient.Client.Send(bt);
        }

        /*
         * Method: EncryptData()
         * Parameters: string
         * Return: String
         * Description: Takes in a string which is ment to be encrypted
         *              and be sent to another program. It encrypts using
         *              a public key provided by target program.
         */
        public string EncryptData(string plaintext)
        {
            rsa.FromXmlString(public_key_client);

            //read plaintext, encrypt it to ciphertext
            byte[] plainbytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
            byte[] cipherbytes = rsa.Encrypt(plainbytes, false);
            return Convert.ToBase64String(cipherbytes);
        }

        /*
         * Method: DecryptData()
         * Parameters: string
         * Return: String
         * Description: Takes in a string which has been sent by another
         *              program and decrypts the sent data. It decrypts using
         *              a private key.
         */
        public string DecryptData(string ciphertext)
        {
            byte[] ciphertextBytes = Convert.FromBase64String(ciphertext);
            rsa.FromXmlString(private_key);

            //read ciphertext, decrypt it to plaintext
            byte[] plaintextBytes = rsa.Decrypt(ciphertextBytes, false);
            return System.Text.Encoding.UTF8.GetString(plaintextBytes);
        }

        /*
         * Method: SendPublicKey()
         * Parameter: nothing
         * Return: void
         * Description: Upon establishing connection to the client,
         *              send it the public key needed to send data to
         *              server.
         */
        public void SendPublicKey()
        {
            byte[] bt;
            bt = Encoding.ASCII.GetBytes(public_key);
            connectedClient.Client.Send(bt);
        }
    }
}
