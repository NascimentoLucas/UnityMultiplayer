using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace UnityMultiPlayer.Common
{
    public class LogHandler : MonoBehaviour
    {

        [Header("Setup")]
        [SerializeField]
        private TextMeshProUGUI _text;
        
        [Header("Debug.Show")]
        [SerializeField]
        private string _log;

        private void Update()
        {
            _text.text = _log;
        }

        public void AddLog(string s)
        {
            _log += s + "\n";
        }

        internal void AddLog(byte[] receivedMessage)
        {
            string msg = Encoding.UTF8.GetString(receivedMessage);
            AddLog(msg);
        }
    }

}