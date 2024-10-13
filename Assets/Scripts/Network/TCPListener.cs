using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityMultiPlayer.Common;
using UnityMultiPlayer.ThreadManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityMultiPlayer.Network
{
    public class Jogador
    {
        private TcpClient cliente;

        private StreamReader reader = null;
        private StreamWriter writer = null;

        public string loginUser;
        public string dados;

        public int id { get; private set; } = -1;

        public Jogador(int id, TcpClient cliente)
        {
            this.id = id;
            this.cliente = cliente;

            NetworkStream stream = this.cliente.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            ThreadController.Instance.StartNewThread(Run);
        }

        public void EnviaMenssagem(string dados)
        {
            writer.WriteLine(dados);
            writer.Flush();
        }

        public void Run()
        {
            Debug.Log($"Start id:{id}.listener");
            string dados = reader.ReadLine();
            while (dados != null)
            {
                try
                {
                    Debug.Log($"{id}: {dados}");
                    dados = reader.ReadLine();
                }
                catch (Exception e)
                {
                    Debug.Log($"Erro de rede: {e}");
                    dados = null;
                }
            }
            cliente.Close();
        }
    }

    public class TCPListener : UnitySingleton<TCPListener>
    {
        private TcpListener _listener;
        private int _port;
        private List<Jogador> _jogadorList = new List<Jogador>();

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
        [MenuItem("Dev/" + nameof(TCPListener) + "/" + nameof(InitTCPListener))]
        public static void InitTCPListener()
        {
            try
            {
                if (!EditorApplication.isPlaying) return;
                GameObject obj = new GameObject(nameof(TCPListener));
                TCPListener tcp = obj.AddComponent<TCPListener>();
                tcp._port = 5000;
                tcp._listener = new TcpListener(IPAddress.Loopback, tcp._port);
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
                _listener.Start();
                Debug.Log($"Listening on port {_port}...");

                while (true)
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    _jogadorList.Add(new Jogador(_jogadorList.Count, client));
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
    }

}