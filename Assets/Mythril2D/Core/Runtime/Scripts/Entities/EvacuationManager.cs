using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class EvacuationManager : MonoBehaviour
    {
        [Serializable]
        public class KeyValueMapping
        {
            public string Key;
            public List<GameObject> Value;

            public KeyValueMapping(string key, List<GameObject> value)
            {
                Key = key;
                Value = value;
            }
        }

        [SerializeField]
        private List<KeyValueMapping> evacuationRules = new List<KeyValueMapping>();

        private void OnEnable()
        {
            GameManager.NotificationSystem.SetActiveEvacuation.AddListener(SetAvailableEvacuation);
        }

        private void OnDestroy()
        {
            GameManager.NotificationSystem.SetActiveEvacuation.RemoveListener(SetAvailableEvacuation);
        }

        public List<GameObject> Get(string key)
        {
            var mapping = evacuationRules.Find(m => m.Key == key);
            return mapping?.Value;
        }

        private void SetAvailableEvacuation(string teleportName)
        {
            foreach (var evacuation in Get(teleportName))
            {
                evacuation.SetActive(false);
            }
        }
    }
}
