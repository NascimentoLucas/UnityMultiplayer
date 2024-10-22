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
    public interface IHandlerMsgReceive
    {
        void HandleMsg(int id, byte[] dados, int length);
    }

    public class JogadorTCP
    {
        private TcpClient _cliente;

        private IHandlerMsgReceive _handler;
        IPEndPoint _endPoint;

        public string loginUser;
        public string dados;

        public int id { get; private set; } = -1;

        public JogadorTCP(int id, TcpClient cliente, IHandlerMsgReceive handler)
        {
            this.id = id;
            this._cliente = cliente;
            this._handler = handler;

            _endPoint = _cliente.Client.RemoteEndPoint as IPEndPoint;
            _endPoint.Port = TCPListener.UDPPort;
            ThreadController.Instance.StartNewThread(Run);
        }

        public void TCPEnviarMenssagem(byte[] dados)
        {
            Debug.Log($"{id}.TCP Send {dados.Length}");
            _cliente.GetStream().Write(dados, 0, dados.Length);
        }

        public void UDPEnviarMenssagem(UdpClient _udp, byte[] dados)
        {
            try
            {
                Debug.Log($"{id}.UDP Send {dados}; {_endPoint.Address}:{_endPoint.Port}");
                _udp.Send(dados, dados.Length, _endPoint);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void Run()
        {
            Debug.Log($"Start id:{id}.listener");
            byte[] buffer = new byte[1024]; // Create a buffer to hold the bytes
            do
            {
                try
                {
                    Debug.Log($"{id}: {dados}");
                    int bytesRead = _cliente.GetStream().Read(buffer, 0, buffer.Length);
                    _handler?.HandleMsg(id, buffer, bytesRead);
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