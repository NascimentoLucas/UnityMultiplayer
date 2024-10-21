#if UNITY_EDITOR
#define LOG
#endif
using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using UnityEngine;
using UnityMultiPlayer.ThreadManagement;
using System.Text;


namespace UnityMultiPlayer.Network
{
    public class JogadorTCP
    {
        private TcpClient _cliente;

        private StreamReader _reader = null;
        private StreamWriter _writer = null;
        private TCPListener _listener;
        IPEndPoint _endPoint;

        public string loginUser;
        public string dados;

        public int id { get; private set; } = -1;

        public JogadorTCP(int id, TcpClient cliente, TCPListener listener)
        {
            this.id = id;
            this._cliente = cliente;
            this._listener = listener;

            NetworkStream stream = this._cliente.GetStream();
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream);
            ThreadController.Instance.StartNewThread(Run);
            _endPoint = _cliente.Client.RemoteEndPoint as IPEndPoint;
            _endPoint.Port = 5001;
        }

        public void TCPEnviarMenssagem(string dados)
        {
            Debug.Log($"{id}.TCP Send {dados}");
            _writer.WriteLine(dados);
            _writer.Flush();
        }

        public void UDPEnviarMenssagem(UdpClient _udp, string dados)
        {
            try
            {
                Debug.Log($"{id}.UDP Send {dados}; {_endPoint.Address}.{_endPoint.Port}");
                byte[] responseData = Encoding.UTF8.GetBytes(dados);
                _udp.Send(responseData, responseData.Length, _endPoint);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void Run()
        {
            Debug.Log($"Start id:{id}.listener");
            do
            {
                try
                {
                    Debug.Log($"{id}: {dados}");
                    dados = _reader.ReadLine();
                    _listener.ShareMsg(id, dados);
                }
                catch (Exception e)
                {
                    Debug.Log($"Erro de rede: {e}");
                    dados = null;
                }
            }
            while (dados != null);

            _cliente.Close();
        }
    }

}