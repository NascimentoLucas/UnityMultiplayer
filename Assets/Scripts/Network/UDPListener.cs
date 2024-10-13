using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using UnityMultiPlayer.ThreadManagement;
using UnityMultiPlayer.Common;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityMultiPlayer.Network
{
    public class JogadorUDP
    {
        public string loginUser;
        public string dados;
        public int Id { get; private set; } = -1;
        public UdpClient UdpListener { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        public JogadorUDP(int id, UdpClient udpListener, IPEndPoint endPoint)
        {
            this.Id = id;
            this.UdpListener = udpListener;
            this.EndPoint = endPoint;
        }

        public void EnviaMenssagem(string dados)
        {
            byte[] responseData = Encoding.UTF8.GetBytes(dados);
            UdpListener.Send(responseData, responseData.Length, EndPoint);  
        }
    }

    public class UDPListener : UnitySingleton<UDPListener>
    {
        private UdpClient _udpListener;
        private int _port;
        private List<JogadorUDP> _jogadorList = new List<JogadorUDP>();

        private void Start()
        {
            bool isServer = false;

#if UNITY_SERVER
            isServer = true;
#endif

            if (isServer
#if UNITY_EDITOR
                || true
#endif
                )
            {
                ThreadController.Instance.StartNewThread(StartListening);
            }
            else
            {
                Destroy(gameObject);
            }

        }

#if UNITY_EDITOR
        [MenuItem("Dev/" + nameof(UDPListener) + "/" + nameof(InitUDPListener))]
        public static void InitUDPListener()
        {
            try
            {
                if (!EditorApplication.isPlaying) return;
                GameObject obj = new GameObject(nameof(UDPListener));
                UDPListener udp = obj.AddComponent<UDPListener>();
                udp._port = 5001;
                udp._udpListener = new UdpClient(udp._port);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
#endif

        public void StartListening()
        {
            try
            {
                Debug.Log($"Listening on port {_port}...");

                while (true)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _port);
                    byte[] receivedData = _udpListener.Receive(ref endPoint);  // Receive data from any IP
                    string receivedMessage = Encoding.UTF8.GetString(receivedData);

                    Debug.Log($"Received message from {endPoint}: {receivedMessage}");

                    // Simulate adding a player (or handling data in a more complex way)
                    _jogadorList.Add(new JogadorUDP(_jogadorList.Count, _udpListener, endPoint));
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
            Debug.Log("Stopped listening.");
        }

        private void OnApplicationQuit()
        {
            StopListening();
        }
    }

}
