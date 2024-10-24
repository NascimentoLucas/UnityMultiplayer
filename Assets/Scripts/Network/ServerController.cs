#if UNITY_EDITOR
#define UNITY_SERVER
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

    public class ServerController : UnitySingleton<ServerController>, IHandlerTCPMsg, IHandlerUdpMsg
    {
        public const int TCPPort = 5000;
        public const int UDPPort = 5001;

        [Header("Setup")]
        [SerializeField]
        private UDPListener _udp;

        [Header("Setup.Log")]
        [SerializeField]
        private TextMeshProUGUI _log;
        private string _logString;
        private TcpListener _listener;
        private List<JogadorTCP> _jogadorList;

        private void Start()
        {
            _listener = new TcpListener(IPAddress.Any, TCPPort);
            ThreadController.Instance.StartNewThread(StartListening);
            _udp.SetHandler(this);
        }

        private void FixedUpdate()
        {
            _log.text = _logString;
        }

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
                Debug.Log($"{nameof(ServerController)} listening on port {TCPPort}...");

                while (true)
                {
                    try
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        AddPlayer(client);
                    }
                    catch (Exception e)
                    {

                        Debug.Log($"{nameof(ServerController)} loop error: {e.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"{nameof(ServerController)} error: {ex.Message}");
            }
        }

        private void AddPlayer(TcpClient client)
        {
            _jogadorList.Add(new JogadorTCP(_jogadorList.Count, client, this));
            int index = _jogadorList.Count - 1;
            _jogadorList[index].TCPEnviarMenssagem(GameController.BytesPlayerConnected(index));
        }

        public void StopListening()
        {
            _listener.Stop();
            Debug.Log($"{nameof(ServerController)} stopped listening.");
        }

        public void HandleUDP(UdpClient udpListener, byte[] receivedMessage, int length)
        {
            for (int i = 0; i < _jogadorList.Count; i++)
            {
                try
                {
                    _logString += $"{receivedMessage.Length}\n";
                    _jogadorList[i].UDPEnviarMenssagem(udpListener, receivedMessage);
                }
                catch (Exception e)
                {
                    Debug.Log($"{nameof(ServerController)}.{nameof(HandleUDP)} err at {i}: {e} ");

                }
            }
        }

        public void HandleTCP(int id, byte[] dados, int length)
        {
            _logString += $"{id}: {dados.Length}\n";

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
                    Debug.LogError($"{nameof(ServerController)}.{nameof(HandleTCP)} err: {e} ");
                }
            }
        }
    }

}