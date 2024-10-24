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
        void HandleUDP(UdpClient udpListener, byte[] receivedMessage, int length);
    }

    public class UDPListener : UnitySingleton<UDPListener>
    {
        private IHandlerUdpMsg _handler;

        private UdpClient _udpListener;

        public UdpClient UdpListener { get => _udpListener; }

        private void Start()
        {
            _udpListener = new UdpClient(ServerController.UDPPort);
            ThreadController.Instance.StartNewThread(StartListening);
        }

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
                Debug.Log($"{nameof(UDPListener)} listening on port {ServerController.UDPPort}...");
                while (true)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ServerController.UDPPort);
                    byte[] receivedData = _udpListener.Receive(ref endPoint);
                    _handler?.HandleUDP(_udpListener, receivedData, receivedData.Length);
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
