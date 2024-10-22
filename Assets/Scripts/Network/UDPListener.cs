#if UNITY_EDITOR
#define LOG
#endif
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using UnityMultiPlayer.ThreadManagement;
using UnityMultiPlayer.Common;
using TMPro;

namespace UnityMultiPlayer.Network
{
    public interface IHandlerUdpMsg
    {
        void Handle(UdpClient udpListener, string receivedMessage);
    }

    public class UDPListener : UnitySingleton<UDPListener>
    {
        private IHandlerUdpMsg _handler;
#if LOG
        [SerializeField]
        private TextMeshProUGUI _log;
        private string _logString; 
#endif

        private UdpClient _udpListener;

        private void Start()
        {
            bool isServer = false;

#if UNITY_SERVER
            isServer = true;
#endif

            if (isServer)
            {
                _udpListener = new UdpClient(TCPListener.UDPPort);
                ThreadController.Instance.StartNewThread(StartListening);
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

        public void SetHandler(IHandlerUdpMsg handler)
        {
            _handler = handler; 
        }

        public void StartListening()
        {
            try
            {
                Debug.Log($"{nameof(UDPListener)} listening on port {TCPListener.UDPPort}...");
                while (true)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, TCPListener.UDPPort);
                    byte[] receivedData = _udpListener.Receive(ref endPoint);
                    string receivedMessage = Encoding.UTF8.GetString(receivedData);

#if LOG
                    _logString += $"{endPoint.Address}: {receivedMessage}\n"; 
#endif
                    _handler?.Handle(_udpListener, receivedMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error: {ex.Message}");
            }
        }

        public void StopListening()
        {
            _udpListener.Close();
            Debug.Log($"{nameof(UDPListener)} Stopped listening.");
        }
    }
}
