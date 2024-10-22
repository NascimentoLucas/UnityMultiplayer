using System;
using TMPro;
using UnityEngine;

namespace UnityMultiPlayer.Game
{
    public class PlayerBehaviour : MonoBehaviour
    {

        [Header("Setup")]
        [SerializeField]
        private TextMeshProUGUI _title;
        private int _index = -1;

        private void Update()
        {
            _title.text = _index.ToString();
            if(_index < 0)
            {
                transform.localScale = Vector3.zero;
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }

        public void Setup(int index)
        {
            _index = index;
        }

    }

}