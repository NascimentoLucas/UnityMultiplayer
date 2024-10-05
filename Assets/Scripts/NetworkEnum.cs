using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public enum NetworkMsgType
{
    movement,
    networkEvent,
}

public class NetworkEnum : MonoBehaviour
{
    const int _enumSize = 4;

    private static void HandleMsg(List<byte> bytes)
    {
        List<byte> enumBytes = bytes.GetRange(0, _enumSize);
        string msg = string.Empty;
        if (bytes.Count > _enumSize + 1)
        {
            int end = bytes.Count - _enumSize;
            if (end > 0)
            {
                List<byte> msgBytes = bytes.GetRange(_enumSize, end);
                msg = Encoding.UTF8.GetString(msgBytes.ToArray()); 
            }
        }

        int result = BitConverter.ToInt32(enumBytes.ToArray(), 0);

        Debug.Log($"{(NetworkMsgType)result}: {msg}");
    }

#if UNITY_EDITOR
    [MenuItem("Dev/" + nameof(NetworkEnum) + "/" + nameof(Test))]
    public static void Test()
    {
        List<byte> bytes = new List<byte>();

        int eSize = System.Enum.GetValues(typeof(NetworkMsgType)).Length;
        int enumAsInt = UnityEngine.Random.Range(0, eSize);
        NetworkMsgType msgType = (NetworkMsgType)enumAsInt;
        bytes.AddRange(BitConverter.GetBytes(enumAsInt));

        string msg = "Hello world: " + UnityEngine.Random.Range(0, 10);
        bytes.AddRange(Encoding.UTF8.GetBytes(msg));


        Debug.Log($"{msgType};{msg}");
        HandleMsg(bytes);
    }
#endif
}
