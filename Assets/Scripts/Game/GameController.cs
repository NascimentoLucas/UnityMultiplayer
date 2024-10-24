using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityMultiPlayer.Common;
using UnityMultiPlayer.Network;

namespace UnityMultiPlayer.Game
{


    public class GameController : MonoBehaviour, INetworkReadHandler, IHandlerUdpMsg
    {
        [Header("Setup")]
        [SerializeField]
        private ClientController _client;
        [SerializeField]
        private UDPListener _udpListener;
        [SerializeField]
        private LoopMessagesController _loop;


        [Header("Setup")]
        [SerializeField]
        private Transform _father;
        [SerializeField]
        private PlayerBehaviour _prefab;
        private PlayerBehaviour _newPlayer;
        private Dictionary<int, PlayerBehaviour> _players = new Dictionary<int, PlayerBehaviour>();


        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            NetworkReaderController.Instance.AddHandler(NetworkMsgType.PlayerConnected, this);
            NetworkReaderController.Instance.AddHandler(NetworkMsgType.NewPlayer, this);
            _udpListener.SetHandler(this);
        }

        private void Update()
        {
            if (_newPlayer == null)
            {
                _newPlayer = Instantiate(_prefab, _father);
            }
        }

        private void FixedUpdate()
        {
            if (_players.ContainsKey(0))
            {
                _players[0].SetTime(DateTime.Now.Second);
                _client.SendMsgUDP(BytesToTime(_players[0].Index, _players[0].Time));
            }
        }

        public void HandleMsg(NetworkMsgType type, byte[] msgBytes)
        {
            int index = NetworkReaderController.GetInt(msgBytes, 0);
            switch (type)
            {
                case NetworkMsgType.PlayerConnected:
                    Debug.Log($"PlayerConnected: {type}.{index}");
                    _players.Add(0, _newPlayer);
                    SetupTemp();

                    byte[] bytes = BytesToNewPlayer(index);
                    _client.SendMsgTCP(bytes);
                    _loop.StartLoop();
                    break;
                case NetworkMsgType.NewPlayer:
                    if (!_players.ContainsKey(index))
                    {
                        _players.Add(index, _newPlayer);
                        SetupTemp();
                        SendMyInfo();
                    }
                    break;
                default:
                    break;
            }
            void SetupTemp()
            {
                _newPlayer.Setup(index);
                _newPlayer = null;
            }
        }

        internal static byte[] BytesPlayerConnected(int index)
        {
            List<byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((int)NetworkMsgType.PlayerConnected));
            result.AddRange(BitConverter.GetBytes(index + 1));
            return result.ToArray();
        }

        internal static byte[] BytesToNewPlayer(int index)
        {
            List<byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((int)NetworkMsgType.NewPlayer));
            result.AddRange(BitConverter.GetBytes(index));
            return result.ToArray();
        }

        internal static byte[] BytesToTime(int index, int time)
        {
            List<byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((int)NetworkMsgType.Time));
            result.AddRange(BitConverter.GetBytes(index));
            result.AddRange(BitConverter.GetBytes(time));
            return result.ToArray();
        }

        public void HandleUDP(UdpClient udpListener, byte[] receivedMessage, int length)
        {
            try
            {
                int index = NetworkReaderController.GetInt(receivedMessage, 4);
                int time = NetworkReaderController.GetInt(receivedMessage, 8);

                if (_players.ContainsKey(index) && _players[0].Index != index)
                {
                    _players[index].SetTime(time);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void SendMyInfo()
        {
            _client.SendMsgTCP(BytesToNewPlayer(_players[0].Index));
        }
    }

}