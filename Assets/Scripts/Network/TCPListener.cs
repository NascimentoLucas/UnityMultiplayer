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


namespace UnityMultiPlayer.Network
{

    public class TCPListener : UnitySingleton<TCPListener>
    {
        public const int TCPPort = 5000;
        public const int UDPPort = 5001;

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
                Init();
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

        private void Init()
        {
            _listener = new TcpListener(IPAddress.Any, TCPPort);
            ThreadController.Instance.StartNewThread(StartListening);
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
                        _jogadorList.Add(new JogadorTCP(_jogadorList.Count, client, this));
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

        public void StopListening()
        {
            _listener.Stop();
            Debug.Log($"{nameof(TCPListener)} stopped listening.");
        }

        internal void ShareMsg(int id, string dados)
        {
#if LOG
            _logString += $"{id}: {dados}\n";
#endif

            for (int i = 0; i < _jogadorList.Count; i++)
            {
                try
                {
                    if (id != i)
                    {
                        _jogadorList[i].TCPEnviarMenssagem(dados);
                    }
                }
                catch (Exception e) 
                {
                    Debug.LogError($"{nameof(TCPListener)}.{nameof(ShareMsg)} err: {e} ");
                }
            }
        }

        internal void ShareAsUDP(UdpClient udp, string dados)
        {
            for (int i = 0; i < _jogadorList.Count; i++)
            {
                try
                {
                    _jogadorList[i].UDPEnviarMenssagem(udp, dados);
                }
                catch (Exception e)
                {
                    Debug.Log($"{nameof(TCPListener)}.{nameof(ShareAsUDP)} err at {i}: {e} ");

                }
            }
        }
    }

}