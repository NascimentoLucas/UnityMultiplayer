using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityMultiPlayer.Network;
using UnityMultiPlayer.ThreadManagement;

namespace UnityMultiPlayer.Common
{

    public class ClientConnection : MonoBehaviour, IHandlerTCPMsg, INetworkReadHandler
    {
        const int port = TCPListener.TCPPort;
        private const string Key = "KeyIpAddress";
        [Header("Setup")]
        [SerializeField]
        private UDPListener _udp;
        [SerializeField]
        private LogHandler _logTcp;
        [SerializeField]
        private LogHandler _logUdp;

        [Header("Setup.Text")]
        [SerializeField]
        private TextMeshProUGUI _connectionInfo;
        [SerializeField]
        private TMP_InputField _ipAddress;

        private JogadorTCP _jogadorTCP;

        private string _logString = string.Empty;
        private bool _connected = false;


        private void Start()
        {
            _connectionInfo.text = GetLocalIPAddress();
            _ipAddress.text = PlayerPrefs.GetString(Key, "192.168.0.29");
        }

        private string GetLocalIPAddress()
        {
            string localIP = "";
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(localIP))
                {
                    Debug.LogError("No network adapters with an IPv4 address in the system!");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error fetching local IP address: " + ex.Message);
            }
            return localIP;
        }

        private void ConnectToServer()
        {
            if (_connected) return;
            NetworkReaderController.Instance.AddHandler(NetworkMsgType.Movement, this);
            _connected = true;
            TcpClient client = new TcpClient(_ipAddress.text, port);
            _jogadorTCP = new JogadorTCP(-10, client, this);
        }

        public void TryConnectToServer()
        {
            if (_connected || _jogadorTCP != null) return;
            ThreadController.Instance.StartNewThread(ConnectToServer);
            PlayerPrefs.SetString(Key, _ipAddress.text);
        }

        public void SendMsgTCP()
        {
            if (!_connected || _jogadorTCP == null) return;

            string msg = $"{_jogadorTCP.id}: TCP msg";
            SendMsgTCP(NetworkReaderController.GetMsg(NetworkMsgType.Movement, msg));
        }

        public void SendMsgUDP()
        {
            if (!_connected || _jogadorTCP == null) return;

            string msg = $"{_jogadorTCP.id}: UDP msg";
            _jogadorTCP.UDPEnviarMenssagem(_udp.UdpListener, NetworkReaderController.GetMsg(NetworkMsgType.Movement, msg));
        }

        public void HandleTCP(int id, byte[] dados, int length)
        {
            NetworkReaderController.Instance.HandleMsg(dados, length);
            _logTcp.AddLog(dados);
        }

        internal void SendMsgTCP(byte[] bytes)
        {
            if (!_connected || _jogadorTCP == null) return;
            _jogadorTCP.TCPEnviarMenssagem(bytes);
        }

        public void HandleMsg(NetworkMsgType type, byte[] msgBytes)
        {
            _logTcp.AddLog(msgBytes);
        }

        internal void SendMsgUDP(byte[] bytes)
        {
            if (!_connected || _jogadorTCP == null) return;
            _jogadorTCP.UDPEnviarMenssagem(_udp.UdpListener, bytes);
        }
    }

}