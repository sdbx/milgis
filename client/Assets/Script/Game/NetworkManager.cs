﻿using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Game
{
    public class NetworkManager : MonoBehaviour
    {

        private GameManager gameManager;
        public string ipAddress = "127.0.0.1";
        public int port = 0;


        private JsonSerializerSettings jsonSettings;

        delegate void EventHandler(Event e);
        private Dictionary<Type, EventHandler> handle;

        private static string receiveData;
        private float connectTime = 0;
        private string invite = "";
        private bool isGameScene = false; 

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded+= new UnityEngine.Events.UnityAction<Scene, LoadSceneMode>(OnSceneLoaded);
        }

        void OnSceneLoaded(Scene scene,LoadSceneMode loadMode)
        {
            if(scene.name == "GameScene")
            {
                gameManager = FindObjectOfType<GameManager>();
                isGameScene = true;
            }
        }

        public void StartManger(string ip,int port, string invite)
        {
            this.ipAddress = ip;
            this.port = port;
            this.invite = invite;

            jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            };
            jsonSettings.Converters.Add(new EventConverter());

            handle = new Dictionary<Type, EventHandler>
        {
            { typeof(GameStartEvent), new EventHandler(GameStartHandler)},
            { typeof(EnterEvent), new EventHandler(EnterHandler)},
            { typeof(MoveEvent), new EventHandler(MoveHandler)},
            { typeof(ConnectedEvent), new EventHandler(ConnectedHandler)}
        };

            AsyncCallbackClient.Instance().connectedCallback = connectedCallback;
            AsyncCallbackClient.Instance().Connect(ipAddress, port);
        }

        void connectedCallback()
        {
            ConnectCommand connectCommand = new ConnectCommand(invite);
            SendCommand(connectCommand);
        }

        void GameStartHandler(Event e)
        {
            GameStartEvent gameStart = (GameStartEvent)e;
            gameManager.StartGame(gameStart.board, gameStart.turn);
        }

        void EnterHandler(Event e)
        {
            EnterEvent Enter = (EnterEvent)e;
        }

        void MoveHandler(Event e)
        {
            MoveEvent Move = (MoveEvent)e;
            if (Move.player != gameManager.myBallType)
            {
                gameManager.OppenetMovement(new BallSelection(Move.start, Move.end), CubeCoord.ConvertDirectionToNum(Move.dir));
            }
        }

        void ConnectedHandler(Event e)
        {
            ConnectedEvent connected = (ConnectedEvent)e;
        }
        void Update()
        {
            if(!isGameScene)
                return;

            if (AsyncCallbackClient.Instance().state == ClientState.DISCONNECTED)
            {
                connectTime += Time.deltaTime;
                if (connectTime > 3.0f)
                {
                    AsyncCallbackClient.Instance().Connect(ipAddress, port);
                    connectTime = 0;
                }
            }

            int dataCount = AsyncCallbackClient.Instance().dataQueue.Count;
            if (dataCount > 0)
            {
                for (int i = 0; i < dataCount; i++)
                {
                    string data = AsyncCallbackClient.Instance().dataQueue.Dequeue();
                    ReceiveData(data);
                }
            }

            int logCount = AsyncCallbackClient.Instance().logQueue.Count;
            if (logCount > 0)
            {
                for (int i = 0; i < logCount; i++)
                {
                    Debug.Log(AsyncCallbackClient.Instance().logQueue.Dequeue());
                }
            }
        }

        public void SendCommand(Command command)
        {
            string json = JsonConvert.SerializeObject(command);
            SendData(json);
        }

        void ReceiveData(string receiveStr)
        {
            foreach (string splitedStr in receiveStr.Split('\n'))
            {
                if (splitedStr == "")
                    return;

                Event e = JsonConvert.DeserializeObject<Event>(splitedStr, jsonSettings);
                handle[e.GetType()](e);
                Debug.Log(e.GetType());
            }
        }

        public static void SendData(string data)
        {
            AsyncCallbackClient.Instance().SendData(data);
        }

        void OnApplicationQuit()
        {
            AsyncCallbackClient.Instance().Close();
        }
    }
}