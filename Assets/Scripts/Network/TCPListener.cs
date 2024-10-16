using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityMultiPlayer.Common;
using UnityMultiPlayer.ThreadManagement;
using TMPro;
using System.Text;
using static UnityEditor.PlayerSettings;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityMultiPlayer.Network
{
    public class JogadorTCP
    {
        private TcpClient _cliente;

        private StreamReader _reader = null;
        private StreamWriter _writer = null;
        private TCPListener _listener;
        private UdpClient _udp;

        public string loginUser;
        public string dados;

        public int id { get; private set; } = -1;

        public JogadorTCP(int id, TcpClient cliente, TCPListener listener)
        {
            this.id = id;
            this._cliente = cliente;
            this._listener = listener;
            Debug.Log($"New TCP {cliente.ExclusiveAddressUse}");

            NetworkStream stream = this._cliente.GetStream();
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream);
            ThreadController.Instance.StartNewThread(Run);
            //_udp = new UdpClient(5001);
        }

        public void TCPEnviarMenssagem(string dados)
        {
            _writer.WriteLine(dados);
            _writer.Flush();
        }

        public void UDPEnviarMenssagem(string dados)
        {
            //if (_cliente.Client.RemoteEndPoint is IPEndPoint endPoint)
            //{
            //    Debug.Log("Send UDP msg: " + dados);
            //    byte[] responseData = Encoding.UTF8.GetBytes(dados);
            //    _udp.Send(responseData, responseData.Length, endPoint);
            //}
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

    public class TCPListener : UnitySingleton<TCPListener>
    {
        private TcpListener _listener;
        private int _port;
        private List<JogadorTCP> _jogadorList;
        [SerializeField]
        private TextMeshProUGUI _log;
        private string _logString;

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
                _port = 5000;
                _listener = new TcpListener(IPAddress.Any, _port);
                ThreadController.Instance.StartNewThread(StartListening);
            }
            else
            {
                Destroy(gameObject);
            }

        }

        private void FixedUpdate()
        {
            _log.text = _logString;
        }

#if UNITY_EDITOR
        [MenuItem("Dev/" + nameof(TCPListener) + "/" + nameof(InitTCPListener))]
        public static void InitTCPListener()
        {
            try
            {
                if (!EditorApplication.isPlaying) return;
                GameObject obj = new GameObject(nameof(TCPListener));
                TCPListener tcp = obj.AddComponent<TCPListener>();
                tcp._port = 5000;
                tcp._listener = new TcpListener(IPAddress.Any, tcp._port);
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
                _jogadorList = new List<JogadorTCP>();
                _listener.Start();
                Debug.Log($"Listening on port {_port}...");

                while (true)
                {
                    try
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        _jogadorList.Add(new JogadorTCP(_jogadorList.Count, client, this));
                    }
                    catch (Exception e)
                    {

                        Debug.Log($"while Error: {e.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error: {ex.Message}");
            }
        }

        public void StopListening()
        {
            _listener.Stop();
            Debug.Log("Stopped listening.");
        }

        private void OnApplicationQuit()
        {
            StopListening();
        }

        internal void ShareMsg(int id, string dados)
        {
            _logString += $"{id}: {dados}\n";

            for (int i = 0; i < _jogadorList.Count; i++)
            {
                try
                {
                    if (id != i)
                    {
                        _jogadorList[i].TCPEnviarMenssagem(dados);
                    }
                }
                catch
                {
                }
            }
        }

        internal void ShareAsUDP(string dados)
        {
            for (int i = 0; i < _jogadorList.Count; i++)
            {
                try
                {
                    _jogadorList[i].UDPEnviarMenssagem(dados);
                }
                catch
                {
                }
            }
        }
    }

}