#if !BESTHTTP_DISABLE_SOCKETIO

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BestHTTP.SocketIO;

namespace BestHTTP.Examples
{
    public sealed class SocketIOChatUGUI : MonoBehaviour
    {
        private readonly TimeSpan TYPING_TIMER_LENGTH = TimeSpan.FromMilliseconds(700);

        private enum ChatStates
        {
            Login,
            Chat
        }
        private SocketManager Manager;
        private ChatStates State;
        private string userName = string.Empty;
        private string message = string.Empty;
        private string chatLog = string.Empty;
        private Vector2 scrollPos;
        private bool isTyping;
        private DateTime lastTypingTime = DateTime.MinValue;
        private List<string> typingUsers = new List<string>();

        //UI
        public GameObject loginScreen;
        public GameObject chatScreen;
        public InputField loginName;

        public Text MessagesTxt;
        public Text UsersTxt;
        public Text TypingTxt;

        public InputField Message;
        public Button SendBt;
        public Button SendImageBt;

        public Button SendCancelBt;
        public Button SendEmailBt;
        public Button SendStartBt;

        public RawImage thumbImg;

        public string NodeName="genericName";
        public string NodeRoom="genericRomm";


        void Start()
        {
            // The current state is Login
            State = ChatStates.Login;

            // Change an option to show how it should be done
            SocketOptions options = new SocketOptions();
            options.AutoConnect = false;
            options.ConnectWith = BestHTTP.SocketIO.Transports.TransportTypes.WebSocket;

            // Create the Socket.IO manager
            //Manager = new SocketManager(new Uri("https://socket-io-chat.now.sh/socket.io/"), options);
            //Manager = new SocketManager(new Uri("http://10.1.2.159:3000/socket.io/"), options);
            Manager = new SocketManager(new Uri("http://127.0.0.1:8080/socket.io/"), options);

            SendBt.onClick.AddListener(SendMessage);
            SendImageBt.onClick.AddListener(SendImage);
            Message.onValueChanged.AddListener(UpdateTyping);

            SendEmailBt.onClick.AddListener(SendEmail);
            SendCancelBt.onClick.AddListener(SendCancel);
            SendStartBt.onClick.AddListener(SendStart);

                // Set up custom chat events
                Manager.Socket.On("serverupdate", OnServerUpdate);
            Manager.Socket.On("chatMessage", OnChatMessage);
            /*Manager.Socket.On("play", OnPlay);
            Manager.Socket.On("start", OnStart);
            Manager.Socket.On("stop", OnStop);

            Manager.Socket.On("submit", OnSubmit);
            Manager.Socket.On("pause", OnPause);
            Manager.Socket.On("email", OnEmail);
            Manager.Socket.On("cancel", OnCancel);
            Manager.Socket.On("thumbnail", OnThumbnail);
            //Manager.Socket.On("new message", OnNewMessage);
            Manager.Socket.On("user joined", OnUserJoined);
            Manager.Socket.On("user left", OnUserLeft);
            //Manager.Socket.On("typing", OnTyping);
            //Manager.Socket.On("stop typing", OnStopTyping);*/

            // The argument will be an Error object.
            Manager.Socket.On(SocketIOEventTypes.Error, (socket, packet, args) => Debug.LogError(string.Format("Error: {0}", args[0].ToString())));

                Manager.Socket.On(SocketIOEventTypes.Connect, (socket, packet, args) => SetUserName());
                // We set SocketOptions' AutoConnect to false, so we have to call it manually.
                Manager.Open();
            
        }

        void OnApplicationQuit()
        {
            // Leaving this sample, close the socket
            Manager.Close();
        }

        void Update()
        {
            string typing = string.Empty;

            if (typingUsers.Count > 0)
            {
                typing += string.Format("{0}", typingUsers[0]);

                for (int i = 1; i < typingUsers.Count; ++i)
                    typing += string.Format(", {0}", typingUsers[i]);

                if (typingUsers.Count == 1)
                    typing += " is typing!";
                else
                    typing += " are typing!";
            }

            TypingTxt.text = typing;
            MessagesTxt.text = chatLog;


            // Stop typing if some time passed without typing
            if (isTyping)
            {
                var typingTimer = DateTime.UtcNow;
                var timeDiff = typingTimer - lastTypingTime;
                if (timeDiff >= TYPING_TIMER_LENGTH)
                {
                    Manager.Socket.Emit("stop typing");
                    isTyping = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) && loginName.text!="" && State==ChatStates.Login)
            {
                
                userName = loginName.text;
                SetUserName();
                
            }

           
        }

        void SetUserName()
        {
            //chatLog += "connected ";

            loginScreen.SetActive(false);
            chatScreen.SetActive(true);
            State = ChatStates.Chat;

            Manager.Socket.Emit("adduser", NodeName, NodeRoom);
        }

        void SendMessage()
        {
            if (string.IsNullOrEmpty(Message.text))
                return;

            Manager.Socket.Emit("sendmessage", "chatMessage", Message.text);

            Message.text = string.Empty;
        }

        void SendCancel()
        {

            Manager.Socket.Emit("play");

            chatLog += "sent play";
        }

        void SendStart()
        {
            Manager.Socket.Emit("start", "1");

            chatLog += "sent start 1";
        }
        void SendEmail()
        {
            Manager.Socket.Emit("email", "jorge@sdsad.com");

            chatLog += "sent email jorge@sdsad.com";
        }

        void SendImage()
        {
            Manager.Socket.Emit("finished", CreateImageString());

            chatLog += "sent image";
        }

        void UpdateTyping(string val)
        {
            if (!isTyping && val!="")
            {
                isTyping = true;
                Manager.Socket.Emit("typing");
            }

            lastTypingTime = DateTime.UtcNow;
        }

        void addParticipantsMessage(Dictionary<string, object> data)
        {
            int numUsers = Convert.ToInt32(data["numUsers"]);

            if (numUsers == 1)
                chatLog += "there's 1 participant\n";
            else
                chatLog += "there are " + numUsers + " participants\n";
        }

        void addChatMessage(Dictionary<string, object> data)
        {
            var username = data["username"] as string;
            var msg = data["message"] as string;

            chatLog += string.Format("{0}: {1}\n", username, msg);
        }

        void AddChatTyping(Dictionary<string, object> data)
        {
            var username = data["username"] as string;

            typingUsers.Add(username);
        }

        void RemoveChatTyping(Dictionary<string, object> data)
        {
            var username = data["username"] as string;

            int idx = typingUsers.FindIndex((name) => name.Equals(username));
            if (idx != -1)
                typingUsers.RemoveAt(idx);
        }

        
        void OnServerUpdate(Socket socket, Packet packet, params object[] args)
        {
            Dictionary<string, object> data = args[0] as Dictionary<string, object>;

            chatLog += "got OnServerUpdate"
                +" type " + data["type"]
                + " who " + data["username"]
                + " room " + data["room"]
                + " rooms " + (data["rooms"] as List<object>)[0] as string
                + " users " + (data["users"] as Dictionary<string, object>).Count
                + " \n";
        }
        
        void OnChatMessage(Socket socket, Packet packet, params object[] args)
        {
            Dictionary<string, object> data = args[0] as Dictionary<string, object>;

            chatLog += "got chat"
                + " who " + data["username"]
                + " data " + data["data"]
                + " \n";
        }

        void OnThumbnail(Socket socket, Packet packet, params object[] args)
        {
            chatLog += "got thumb message \n";

            byte[] imageBytes = Convert.FromBase64String(args[0].ToString());
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);

            thumbImg.texture = tex;

            //addParticipantsMessage(args[0] as Dictionary<string, object>);
        }

        void OnNewMessage(Socket socket, Packet packet, params object[] args)
        {
            addChatMessage(args[0] as Dictionary<string, object>);
        }

        string CreateImageString()
        {
            Texture2D texture = new Texture2D(128, 128);

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color color = ((x & y) != 0 ? Color.white : Color.gray);
                    if (color == Color.white)
                        color = (UnityEngine.Random.value > 0.5f) ? Color.blue : Color.red;
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();

            return System.Convert.ToBase64String(texture.EncodeToPNG());
        }

        void OnUserJoined(Socket socket, Packet packet, params object[] args)
        {
            var data = args[0] as Dictionary<string, object>;

            var username = data["username"] as string;

            chatLog += string.Format("{0} joined\n", username);

            addParticipantsMessage(data);
        }

        void OnUserLeft(Socket socket, Packet packet, params object[] args)
        {
            var data = args[0] as Dictionary<string, object>;

            var username = data["username"] as string;

            chatLog += string.Format("{0} left\n", username);

            addParticipantsMessage(data);
        }

        void OnTyping(Socket socket, Packet packet, params object[] args)
        {
            AddChatTyping(args[0] as Dictionary<string, object>);
        }

        void OnStopTyping(Socket socket, Packet packet, params object[] args)
        {
            RemoveChatTyping(args[0] as Dictionary<string, object>);
        }

    }
}

#endif