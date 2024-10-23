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
        private ClientConnection _client;
        [SerializeField]
        private UDPListener _udpListener;


        [Header("Setup")]
        [SerializeField]
        private Transform _father;
        [SerializeField]
        private PlayerBehaviour _prefab;
        private PlayerBehaviour _temp;
        private PlayerBehaviour _destroyIt;
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
            if (_temp == null)
            {
                _temp = Instantiate(_prefab, _father);
            }
            if (_destroyIt != null)
            {
                Destroy(_destroyIt.gameObject);
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
                    _players.Add(0, _temp);
                    SetupTemp();

                    byte[] bytes = BytesToNewPlayer(index);
                    _client.SendMsgTCP(bytes);
                    break;
                case NetworkMsgType.NewPlayer:
                    if (!_players.ContainsKey(index))
                    {
                        _players.Add(index, _temp);
                        SetupTemp();
                        _client.SendMsgTCP(BytesToNewPlayer(_players[0].Index));
                    }
                    break;
                default:
                    break;
            }
            void SetupTemp()
            {
                _temp.Setup(index);
                _temp = null;
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
    }

}