﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.MessageModels;
using DejarikLibrary;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class Client : NetworkBehaviour
    {

        public bool IsHost = false;
        //TODO: Net make this private and abstract necessary functionality out
        public NetworkClient NetClient;
        public string ClientName = "client";
        private ClientGameState _gameState;

        public void Init(string hostAddress, int port)
        {
            DontDestroyOnLoad(this);

            if (!IsHost)
            {
                ClientName = "guest";
            }
                        
            try
            {
                NetClient = new NetworkClient();

                NetClient.RegisterHandler(MsgType.Connect, OnConnected);

                NetClient.RegisterHandler(MsgType.Disconnect, OnDisconnected);

                NetClient.RegisterHandler(MsgType.Error, OnError);

                NetClient.RegisterHandler(CustomMessageTypes.GameStart, OnGameStart);

                NetClient.RegisterHandler(CustomMessageTypes.SelectMonsterResponse, OnSelectMonster);

                NetClient.RegisterHandler(CustomMessageTypes.AvailableMovesResponse, OnAvailableMoves);

                NetClient.RegisterHandler(CustomMessageTypes.SelectActionResponse, OnSelectAttackAction);

                NetClient.Connect(hostAddress, port);

                Debug.Log("Client");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

        }

        public void InitHost()
        {

            IsHost = true;
            ClientName = "host";
            try
            {
                Init("127.0.0.1", 1300);
                Debug.Log("HostClient");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

        }

        public bool Send(short messageType, GameStateMessage message)
        {
            return NetClient.Send(messageType, message);
        }

        public void SetReady(bool isReady)
        {
            NetClient.connection.isReady = isReady;
        }

        private void OnConnected(NetworkMessage netMsg)
        {
            Debug.Log("Connected to server");
        }

        private void OnDisconnected(NetworkMessage msg)
        {
            Debug.Log("Disconnected from server");
        }

        private void OnError(NetworkMessage msg)
        {
            Debug.Log("Error connecting with code " + msg.reader.ReadString());
        }

        private void OnSelectMonster(NetworkMessage msg)
        {
            Debug.Log("Monster selected");
            SelectMonsterResponseMessage message = msg.ReadMessage<SelectMonsterResponseMessage>();

            _gameState.ConfirmSubActionTwo(message.SelectedMonsterTypeId, message.ActionNumber, message.SubActionNumber);

        }

        private void OnAvailableMoves(NetworkMessage msg)
        {
            Debug.Log("Available moves calculated");
            AvailableMovesResponseMessage message = msg.ReadMessage<AvailableMovesResponseMessage>();

            _gameState.ConfirmSubActionThree(message.AvailableMoveNodeIds.ToList(), message.AvailableAttackNodeIds.ToList(), message.ActionNumber, message.SubActionNumber);

        }

        private void OnSelectMoveAction(NetworkMessage msg)
        {
            Debug.Log("Action selected");

            SelectMoveResponseMessage message = msg.ReadMessage<SelectMoveResponseMessage>();

            _gameState.ConfirmSelectMoveAction(message.MovementPathIds, message.DestinationNodeId, message.ActionNumber, message.SubActionNumber);
        }

        private void OnSelectAttackAction(NetworkMessage msg)
        {
            Debug.Log("Action selected");

            SelectAttackResponseMessage message = msg.ReadMessage<SelectAttackResponseMessage>();

            _gameState.ConfirmSelectAttackAction(message.AttackNodeId, message.ActionNumber, message.SubActionNumber);
        }

        private void OnAttackResult(NetworkMessage msg)
        {
            Debug.Log("Attack Result");

            AttackResponseMessage message = msg.ReadMessage<AttackResponseMessage>();

            _gameState.ConfirmAttackResult((AttackResult)message.AttackResultId, message.AttackingMonsterTypeId, message.DefendingMonsterTypeId, message.XCoordinate, message.YCoordinate, message.ZCoordinate);
        }

        private void OnGameStart(NetworkMessage netMsg)
        {
            GameStartMessage gameStartMessage = netMsg.ReadMessage<GameStartMessage>();

            //Convert json strings to objects
            Dictionary<int, int> friendlyMonsterIds = IsHost? JsonConvert.DeserializeObject<Dictionary<int, int>>(gameStartMessage.HostMonsters) : JsonConvert.DeserializeObject<Dictionary<int, int>>(gameStartMessage.GuestMonsters);
            Dictionary<int, int> enemyMonsterIds = IsHost ? JsonConvert.DeserializeObject<Dictionary<int, int>>(gameStartMessage.GuestMonsters) : JsonConvert.DeserializeObject<Dictionary<int, int>>(gameStartMessage.HostMonsters);

            GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            gameManager.FriendlyMonsterInitialNodeIds = friendlyMonsterIds;
            gameManager.EnemyMonsterInitialNodeIds = enemyMonsterIds;

            StartCoroutine(LoadGameScene());
        }

        IEnumerator LoadGameScene()
        {
            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync("dejarik");

            while (!loadSceneOperation.isDone)
            {
                Debug.Log("Loading the Game Scene");
                yield return null;
            }

        }


    }
}
