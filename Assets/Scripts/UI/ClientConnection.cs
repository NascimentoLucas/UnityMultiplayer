using System;
using System.Net.Sockets;
using UnityEngine;
using UnityMultiPlayer.Network;
using UnityMultiPlayer.ThreadManagement;

namespace UnityMultiPlayer.Common
{

    public class ClientConnection : MonoBehaviour, IHandlerUdpMsg, IHandlerTCPMsg, INetworkReadHandler
    {
        const string serverIp = TCPListener.ServerIPAddress;
        const int port = TCPListener.TCPPort;



        [Header("Setup")]
        [SerializeField]
        private UDPListener _udp;
        [SerializeField]
        private LogHandler _logTcp;
        [SerializeField]
        private LogHandler _logUdp;
        private JogadorTCP _jogadorTCP;

        private string _logString = string.Empty;
        private bool _connected = false;

        private void ConnectToServer()
        {
            if (_connected) return;
            NetworkReaderController.Instance.AddHandler(NetworkMsgType.Movement, this);
            _connected = true;
            TcpClient client = new TcpClient(serverIp, port);
            _jogadorTCP = new JogadorTCP(-10, client, this);
            _udp.SetHandler(this);

        }

        public void TryConnectToServer()
        {
            if (_connected || _jogadorTCP != null) return;
            ThreadController.Instance.StartNewThread(ConnectToServer);
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

        public void HandleUDP(UdpClient udpListener, byte[] receivedMessage, int length)
        {
            NetworkReaderController.Instance.HandleMsg(receivedMessage, length);
            _logUdp.AddLog(receivedMessage);
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
    }

}