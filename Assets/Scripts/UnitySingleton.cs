using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMultiPlayer.Common
{
    public abstract class UnitySingleton<T> : MonoBehaviour
    {
        public static UnitySingleton<T> Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
    } 
}