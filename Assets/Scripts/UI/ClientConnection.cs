using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityMultiPlayer.Network;
using UnityMultiPlayer.ThreadManagement;

namespace UnityMultiPlayer.Common
{
    public class ClientConnection : MonoBehaviour
    {
        const string serverIp = TCPListener.ServerIPAddress;
        const int port = TCPListener.TCPPort;
        private JogadorTCP _jogadorTCP;
        //private UdpClient _udpListener = new UdpClient(TCPListener.UDPPort + 100);

        private bool _connected = false;


        private void ConnectToServer()
        {
            if (_connected) return;
            _connected = true;
            TcpClient client = new TcpClient(serverIp, port);
            _jogadorTCP = new JogadorTCP(-10, client);
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

            //_jogadorTCP.UDPEnviarMenssagem(_udpListener, $"{_jogadorTCP.id}: UDP msg");
        }
    }

}