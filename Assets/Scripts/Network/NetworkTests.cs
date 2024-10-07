#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityMultiPlayer.Network;

namespace UnityMultiPlayer.Dev
{
    public class NetworkTests : INetworkReadHandler
    {
        private NetworkMsgType _type;

        public NetworkTests(NetworkMsgType type)
        {
            _type = type;
        }

        [MenuItem("Dev/" + nameof(NetworkTests) + "/" + nameof(Test))]
        public static void Test()
        {

            int eSize = System.Enum.GetValues(typeof(NetworkMsgType)).Length;
            int enumAsInt = UnityEngine.Random.Range(0, eSize);
            NetworkMsgType msgType = (NetworkMsgType)enumAsInt;
            try
            {
                NetworkTests t = new NetworkTests(msgType);
                NetworkReaderController.Instance?.AddHandler(msgType, t);
                t.SendMsg();
            }
            catch (ArgumentException)
            {

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SendMsg()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes((int)_type));

            string msg = "Hello world: " + UnityEngine.Random.Range(0, 10);
            bytes.AddRange(Encoding.UTF8.GetBytes(msg));

            Debug.Log($"Send: {_type};{msg}");
            NetworkReaderController.Instance?.HandleMsg(bytes);
        }


        public void HandleMsg(List<byte> bytes)
        {
            string msg = Encoding.UTF8.GetString(bytes.ToArray());
            Debug.Log($"Got: {_type}: {msg}");
        }

    } 
}
#endif
