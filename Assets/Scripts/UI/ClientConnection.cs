using System;
using System.Net.Sockets;
using UnityEngine;
using UnityMultiPlayer.Network;
using UnityMultiPlayer.ThreadManagement;

namespace UnityMultiPlayer.Common
{
    public class ClientConnection : MonoBehaviour, IHandlerUdpMsg, IHandlerMsgReceive
    {
        const string serverIp = TCPListener.ServerIPAddress;
        const int port = TCPListener.TCPPort;



        [Header("Setup")]
        [SerializeField]
        private UDPListener _udp;

        private JogadorTCP _jogadorTCP;
        private bool _connected = false;


        private void ConnectToServer()
        {
            if (_connected) return;
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
            SendMsgTCP(NetworkReaderController.GetMsg(NetworkMsgType.PlayerConnected, msg));
        }

        public void SendMsgUDP()
        {
            if (!_connected || _jogadorTCP == null) return;

            string msg = $"{_jogadorTCP.id}: UDP msg";
            _jogadorTCP.UDPEnviarMenssagem(_udp.UdpListener, NetworkReaderController.GetMsg(NetworkMsgType.PlayerConnected, msg));
        }

        public void Handle(UdpClient udpListener, byte[] receivedMessage, int length)
        {
            NetworkReaderController.Instance.HandleMsg(receivedMessage, length);
        }

        public void HandleMsg(int id, byte[] dados, int length)
        {
            NetworkReaderController.Instance.HandleMsg(dados, length);
        }

        internal void SendMsgTCP(byte[] bytes)
        {
            if (!_connected || _jogadorTCP == null) return;
            _jogadorTCP.TCPEnviarMenssagem(bytes);
        }
    }

}