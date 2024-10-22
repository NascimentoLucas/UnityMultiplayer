using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMultiPlayer.Network;

namespace UnityMultiPlayer.Game
{

    public class GameController : MonoBehaviour, INetworkReadHandler
    {
        [Header("Setup")]
        [SerializeField]
        private UDPListener _udpListener;


        [Header("Setup")]
        [SerializeField]
        private Transform _father;
        [SerializeField]
        private PlayerBehaviour _prefab;
        private PlayerBehaviour _temp;
        private Dictionary<int, PlayerBehaviour> _players = new Dictionary<int, PlayerBehaviour>();


        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            NetworkReaderController.Instance.AddHandler(NetworkMsgType.PlayerConnected, this);
            NetworkReaderController.Instance.AddHandler(NetworkMsgType.NewPlayer, this);
        }

        private void Update()
        {
            if (_temp == null)
            {
                _temp = Instantiate(_prefab, _father);
                _temp.gameObject.SetActive(false);
            }
        }

        public void HandleMsg(NetworkMsgType type, byte[] msgBytes)
        {
            Debug.Log($"Game: {type}");
            switch (type)
            {
                case NetworkMsgType.PlayerConnected:
                    int localPlayerIndex = GetInt(0);
                    Debug.Log($"PlayerConnected: {type}.{localPlayerIndex}");
                    _players.Add(0, _temp);
                    SetupTemp();
                    break;
                case NetworkMsgType.NewPlayer:
                    int index = GetInt(0);
                    if (!_players.ContainsKey(index))
                    {
                        _players.Add(index, _temp);
                    }
                    else
                    {
                        if (_players[index] != null)
                        {
                            Destroy(_players[index].gameObject);
                        }
                        _players[index] = _temp;
                    }
                    SetupTemp();
                    break;
                default:
                    break;
            }

            int GetInt(int startIndex)
            {
                return (msgBytes[startIndex]) |
                                (msgBytes[startIndex + 1] << 8) |
                                (msgBytes[startIndex + 2] << 16) |
                                (msgBytes[startIndex + 3] << 24);
            }
        }

        private void SetupTemp()
        {
            _temp = null;
            _temp.gameObject.SetActive(true);
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
            result.AddRange(BitConverter.GetBytes(index + 1));
            return result.ToArray();
        }
    }

}