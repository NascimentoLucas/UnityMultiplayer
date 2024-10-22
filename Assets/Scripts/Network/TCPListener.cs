#if UNITY_EDITOR
#define LOG
#endif
using System;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Collections.Generic;
using UnityMultiPlayer.Common;
using UnityMultiPlayer.ThreadManagement;
using TMPro;
using UnityMultiPlayer.Game;


namespace UnityMultiPlayer.Network
{

    public class TCPListener : UnitySingleton<TCPListener>, IHandlerMsgReceive, IHandlerUdpMsg
    {
        public const string ServerIPAddress = "192.168.0.4";
        public const int TCPPort = 5000;
        public const int UDPPort = 5001;

        [Header("Setup")]
        [SerializeField]
        private UDPListener _udp;
#if LOG
        [Header("Setup")]
        [SerializeField]
        private TextMeshProUGUI _log;
        private string _logString;
#endif
        private TcpListener _listener;
        private List<JogadorTCP> _jogadorList;

        private void Start()
        {
            bool isServer = false;

#if UNITY_SERVER
            isServer = true;
#endif

            if (isServer)
            {
                _listener = new TcpListener(IPAddress.Any, TCPPort);
                ThreadController.Instance.StartNewThread(StartListening);
                _udp.SetHandler(this);
            }
            else
            {
                Destroy(gameObject);
            }

        }

#if LOG
        private void FixedUpdate()
        {
            _log.text = _logString;
        }
#endif

        private void OnApplicationQuit()
        {
            StopListening();
        }

        public void StartListening()
        {
            try
            {
                _jogadorList = new List<JogadorTCP>();
                _listener.Start();
                Debug.Log($"{nameof(TCPListener)} listening on port {TCPPort}...");

                while (true)
                {
                    try
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        AddJogador(new JogadorTCP(_jogadorList.Count, client, this));
                    }
                    catch (Exception e)
                    {

                        Debug.Log($"{nameof(TCPListener)} loop error: {e.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"{nameof(TCPListener)} error: {ex.Message}");
            }
        }

        private void AddJogador(JogadorTCP jog)
        {
            _jogadorList.Add(jog);
            int index = _jogadorList.Count;
            jog.TCPEnviarMenssagem(GameController.BytesPlayerConnected(index));
            byte[] bytes = GameController.BytesToNewPlayer(index);
            HandleMsg(index, bytes, bytes.Length);
        }

        public void StopListening()
        {
            _listener.Stop();
            Debug.Log($"{nameof(TCPListener)} stopped listening.");
        }

        public void Handle(UdpClient udpListener, byte[] receivedMessage, int length)
        {
            for (int i = 0; i < _jogadorList.Count; i++)
            {
                try
                {
                    _jogadorList[i].UDPEnviarMenssagem(udpListener, receivedMessage);
                }
                catch (Exception e)
                {
                    Debug.Log($"{nameof(TCPListener)}.{nameof(Handle)} err at {i}: {e} ");

                }
            }
        }

        public void HandleMsg(int id, byte[] dados, int length)
        {
#if LOG
            _logString += $"{id}: {dados}\n";
#endif

            for (int i = 0; i < _jogadorList.Count; i++)
            {
                try
                {
                    if (i != id)
                    {
                        _jogadorList[i].TCPEnviarMenssagem(dados);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{nameof(TCPListener)}.{nameof(HandleMsg)} err: {e} ");
                }
            }
        }
    }

}