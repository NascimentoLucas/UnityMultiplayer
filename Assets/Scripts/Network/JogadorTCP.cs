﻿#if UNITY_EDITOR
#define LOG
#endif
using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using UnityEngine;
using UnityMultiPlayer.ThreadManagement;
using System.Text;
using System.Runtime.InteropServices.ComTypes;


namespace UnityMultiPlayer.Network
{
    public interface IHandlerTCPMsg
    {
        void HandleTCP(int id, byte[] dados, int length);
    }

    public class JogadorTCP
    {
        private TcpClient _cliente;

        private IHandlerTCPMsg _handler;
        private IPEndPoint _endPoint;
        public string dados;

        public int Id { get; private set; } = -1;

        public JogadorTCP(int id, TcpClient cliente, IHandlerTCPMsg handler)
        {
            this.Id = id;
            this._cliente = cliente;
            this._handler = handler;

            _endPoint = _cliente.Client.RemoteEndPoint as IPEndPoint;
            _endPoint.Port = ServerController.UDPPort;
            ThreadController.Instance.StartNewThread(Run);
        }

        public void TCPEnviarMenssagem(byte[] dados)
        {
            Debug.Log($"{DateTime.Now.ToString("HH:mm:ss")} => {Id}: {dados.Length}");
            _cliente.GetStream().Write(dados, 0, dados.Length);
        }

        public void UDPEnviarMenssagem(UdpClient _udp, byte[] dados)
        {
            try
            {
                _udp.Send(dados, dados.Length, _endPoint);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void Run()
        {
            Debug.Log($"Start id:{Id}.TCP_Read");
            byte[] buffer = new byte[1024];
            do
            {
                try
                {
                    int bytesRead = _cliente.GetStream().Read(buffer, 0, buffer.Length);
                    _handler?.HandleTCP(Id, buffer, bytesRead);
                }
                catch (Exception e)
                {
                    Debug.Log($"Erro de rede: {e}");
                    dados = null;
                }
            }
            while (true);
        }
    }

}