using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityMultiPlayer.Network;
using UnityMultiPlayer.ThreadManagement;

namespace UnityMultiPlayer.Common
{
    public class ClientConnection : MonoBehaviour, IHandlerUdpMsg
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
            _jogadorTCP = new JogadorTCP(-10, client);
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

            _jogadorTCP.TCPEnviarMenssagem($"{_jogadorTCP.id}: TCP msg");
        }

        public void SendMsgUDP()
        {
            if (!_connected || _jogadorTCP == null) return;

            _jogadorTCP.UDPEnviarMenssagem(_udp.UdpListener, $"{_jogadorTCP.id}: UDP msg");
        }

        public void Handle(UdpClient udpListener, string receivedMessage)
        {
            Debug.Log($"udp: {receivedMessage}");
        }
    }

}