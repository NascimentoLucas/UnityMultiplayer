using System;
using System.Collections.Generic;
using UnityEngine;
using UnityMultiPlayer.Common;


public enum NetworkMsgType
{
    movement,
    networkEvent,
}

public interface INetworkReadHandler
{
    void HandleMsg(List<byte> msgBytes);
}

namespace UnityMultiPlayer.Network
{
    public class NetworkReaderController : UnitySingleton<NetworkReaderController>
    {
        public const int EnumSize = 4;

        Dictionary<NetworkMsgType, INetworkReadHandler> _handlers = new Dictionary<NetworkMsgType, INetworkReadHandler>();

        public void HandleMsg(List<byte> bytes)
        {
            List<byte> enumBytes = bytes.GetRange(0, EnumSize);
            int result = BitConverter.ToInt32(enumBytes.ToArray(), 0);
            var type = (NetworkMsgType)result;

            List<byte> msgBytes = new List<byte>();
            int end = bytes.Count - EnumSize;
            if (end > 0)
            {
                msgBytes = bytes.GetRange(EnumSize, end);
            }

            if (_handlers.ContainsKey(type))
                _handlers[type].HandleMsg(msgBytes);
            else
                throw new NotImplementedException($"Not implemented to handle {type}");
        }

        public void AddHandler(NetworkMsgType type, INetworkReadHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException($"{nameof(handler)} is null");

            if (!_handlers.ContainsKey(type))
                _handlers.Add(type, handler);
            else
                throw new ArgumentException($"Already have {type} handler");
        }
    }

}