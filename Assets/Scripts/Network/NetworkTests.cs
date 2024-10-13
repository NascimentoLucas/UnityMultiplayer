#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityMultiPlayer.Network;
using UnityMultiPlayer.ThreadManagement;

namespace UnityMultiPlayer.Dev
{
    public class NetworkTests : INetworkReadHandler
    {
        private NetworkMsgType _type;
        private float _randomValue;

        public NetworkTests(NetworkMsgType type)
        {
            _type = type;
            _randomValue = UnityEngine.Random.Range(0, 10);
        }

        [MenuItem("Dev/" + nameof(NetworkTests) + "/" + nameof(CreateRandomSenderAndReader))]
        public static void CreateRandomSenderAndReader()
        {
            int eSize = System.Enum.GetValues(typeof(NetworkMsgType)).Length;
            int enumAsInt = UnityEngine.Random.Range(0, eSize);
            NetworkMsgType msgType = (NetworkMsgType)enumAsInt;
            try
            {
                NetworkTests t = new NetworkTests(msgType);
                NetworkReaderController.Instance?.AddHandler(msgType, t);
                ThreadController.Instance?.StartNewThread(t.CallHandleMsg);
            }
            catch (ArgumentException arg)
            {
                Debug.Log(arg.Message);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [MenuItem("Dev/" + nameof(NetworkTests) + "/" + nameof(CreateRandomReader))]
        public static void CreateRandomReader()
        {

            int eSize = System.Enum.GetValues(typeof(NetworkMsgType)).Length;
            int enumAsInt = UnityEngine.Random.Range(0, eSize);
            NetworkMsgType msgType = (NetworkMsgType)enumAsInt;
            try
            {
                NetworkTests t = new NetworkTests(msgType);
                NetworkReaderController.Instance?.AddHandler(msgType, t);
            }
            catch (ArgumentException arg)
            {
                Debug.Log(arg.Message);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void CallHandleMsg()
        {
            while (true)
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes((int)_type));

                string msg = "Hello world: " + _randomValue++;
                bytes.AddRange(Encoding.UTF8.GetBytes(msg));

                Debug.Log($"Send: {_type};{msg}");
                NetworkReaderController.Instance?.HandleMsg(bytes);
                Thread.Sleep(100); 
            }
        }


        public void HandleMsg(List<byte> bytes)
        {
            string msg = Encoding.UTF8.GetString(bytes.ToArray());
            Debug.Log($"Got: {_type}: {msg}");
        }

    } 
}
#endif
