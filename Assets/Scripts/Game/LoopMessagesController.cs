using UnityEngine;

namespace UnityMultiPlayer.Game
{
    public class LoopMessagesController : MonoBehaviour
    {

        [Header("Setup")]
        [SerializeField]
        private GameController _game;
        [SerializeField]
        private float _delay = 5;
        private float _time = 0;
        private bool _running = false;
        private float _appTime;



        private void Start()
        {
            _running = false;
        }

        private void Update()
        {
            _appTime = Time.timeSinceLevelLoad;
            if (!_running) return;

            if (_time < _appTime)
            {
                SetTime();
                _game.SendMyInfo();
            }
        }

        private void SetTime()
        {
            _time = _appTime + _delay;
        }

        public void StartLoop()
        {
            SetTime();
            _running = true;
        }
    }

}