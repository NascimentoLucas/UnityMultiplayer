using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityMultiPlayer.Common;


namespace UnityMultiPlayer.Network
{
    public enum NetworkMsgType
    {
        Movement,
        PlayerConnected,
        NewPlayer,
        Time
    }

    public interface INetworkReadHandler
    {
        void HandleMsg(NetworkMsgType type, byte[] msgBytes);
    }

    public class NetworkReaderController : UnitySingleton<NetworkReaderController>
    {
        public const int EnumSize = 4;

        Dictionary<NetworkMsgType, INetworkReadHandler> _handlers = new Dictionary<NetworkMsgType, INetworkReadHandler>();


        public void HandleMsg(byte[] bytes, int length)
        {
            byte[] enumBytes = new byte[EnumSize];
            Array.Copy(bytes, enumBytes, EnumSize);

            int result = BitConverter.ToInt32(enumBytes, 0);
            var type = (NetworkMsgType)result;

            byte[] msgBytes = new byte[length - EnumSize];
            int end = length - EnumSize;
            if (end > 0)
            {
                Array.Copy(bytes, EnumSize, msgBytes, 0, end);
            }

            if (_handlers.ContainsKey(type))
                _handlers[type].HandleMsg(type, msgBytes);
            else
                throw new NotImplementedException($"Not implemented to handle {type}");
        }

        public void AddHandler(NetworkMsgType type, INetworkReadHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException($"{nameof(handler)} is null");

            if (!_handlers.ContainsKey(type) || _handlers[type] == null)
                _handlers.Add(type, handler);
            else
                throw new ArgumentException($"Already have {type} handler");
        }

        public static byte[] GetMsg(NetworkMsgType type, string msg)
        {
            List<byte> bytes = new List<byte>();

            int typeInt = (int)type;
            bytes.AddRange(BitConverter.GetBytes(typeInt));
            bytes.AddRange(Encoding.ASCII.GetBytes(msg));


            return bytes.ToArray();
        }

        public static int GetInt(byte[] bytes, int index)
        {
            return (bytes[index]) |
                            (bytes[index + 1] << 8) |
                            (bytes[index + 2] << 16) |
                            (bytes[index + 3] << 24);
        }


#if UNITY_EDITOR
        [MenuItem("Dev/" + nameof(NetworkReaderController) + "/" + nameof(LogTypesSize))]
        public static void LogTypesSize()
        {
            int typeInt = (int)NetworkMsgType.Movement;
            Debug.Log(BitConverter.GetBytes(typeInt).Length);
            string msg = $"{1}: TCP msg";
            Debug.Log(NetworkReaderController.GetMsg(NetworkMsgType.Movement, msg).Length);
        }
#endif
    }

}