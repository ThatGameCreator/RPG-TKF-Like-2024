using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gyvr.Mythril2D
{
    public class MapLoadingDelegationParams
    {
        public Action<Action> unloadDelegate;
        public Action<Action> loadDelegate;
        public Action completionDelegate;
    }

    public class MapLoadingSystem : AGameSystem
    {
        [Header("Settings")]
        // 如果委托传送，则会先执行存档在执行传送
        [SerializeField] private bool m_delegateTransitionResponsability = false;
        [SerializeField] private GameObject m_defaultResurrectionPostion = null;
        [SerializeField] private string m_defaultResurrectionMap = null;

        private string m_currentMap = string.Empty;
        private Vector2 m_currentResurrectionPostion = new Vector2();

        public void SetSaveResurrectionPostion(Vector2 postion)
        {
            m_currentResurrectionPostion = postion;
        }

        public void SetActiveMap(string map)
        {
            m_currentMap = map;
        }

        public string GetCurrentMapName()
        {
            return m_currentMap;
        }

        public bool HasCurrentMap()
        {
            return m_currentMap != string.Empty;
        }

        public void RequestTransition(string map = null, Action onMapUnloaded = null, Action onMapLoaded = null, Action onCompletion = null, bool hasDestionationPosition = false)
        {
            if(map == null)
            {
                SetActiveMap(m_defaultResurrectionMap);
            }
            
            // 传送地图也要设置当前地图
            // 好像在这里设置会出问题
            //SetActiveMap(map);

            GameManager.NotificationSystem.mapTransitionStarted.Invoke();

            if (map != GameManager.MapLoadingSystem.GetCurrentMapName())
            {
                if (m_delegateTransitionResponsability)
                {
                    DelegateTransition(map, onMapUnloaded, onMapLoaded, onCompletion);
                }
                else
                {
                    ExecuteTransition(map, onMapUnloaded, onMapLoaded, onCompletion);
                }
            }

            if(hasDestionationPosition == true)
            {
                TeloportPlayerPosition();
            }
        }

        private void TeloportPlayerPosition()
        {
            if (m_currentResurrectionPostion == null)
            {
                GameManager.Player.transform.position = m_defaultResurrectionPostion.transform.position;
            }
            else
            {
                GameManager.Player.transform.position = m_currentResurrectionPostion;
            }
        }

        private void DelegateTransition(string map, Action onMapUnloaded = null, Action onMapLoaded = null, Action onCompletion = null)
        {
            GameManager.NotificationSystem.mapTransitionDelegationRequested.Invoke(new MapLoadingDelegationParams
            {
                unloadDelegate = HasCurrentMap() ? (callback) => UnloadMap(m_currentMap, callback + onMapUnloaded) : null,
                loadDelegate = (callback) => LoadMap(map, callback + onMapLoaded),
                completionDelegate = () => CompleteTransition(onCompletion)
            });
        }

        private void ExecuteTransition(string map, Action onMapUnloaded = null, Action onMapLoaded = null, Action onCompletion = null)
        {
            Action loadAction =
                () => LoadMap(map,
                (() => CompleteTransition(onCompletion)) + onMapLoaded);

            if (HasCurrentMap())
            {
                UnloadMap(m_currentMap, loadAction);
            }
            else
            {
                loadAction();
            }
        }

        private void UnloadMap(string map, Action onCompletion)
        {
            // if teleported map equal to current map then can not teleport
            if (map != string.Empty && map == m_currentMap)
            {
                Debug.Log($"Unloading Map {map}...");

                AsyncOperation operation = SceneManager.UnloadSceneAsync(map);

                operation.completed += (op) =>
                {
                    GameManager.NotificationSystem.mapUnloaded.Invoke();
                    onCompletion?.Invoke();
                };
            }
            else
            {
                GameManager.NotificationSystem.mapUnloaded.Invoke();
                onCompletion?.Invoke();
            }
        }

        private void LoadMap(string map, Action onCompletion)
        {
            if (map != string.Empty && map != m_currentMap)
            {
                Debug.Log($"Loading Map {map}...");

                AsyncOperation operation = SceneManager.LoadSceneAsync(map, LoadSceneMode.Additive);

                operation.completed += (op) =>
                {
                    Debug.Log("SetActiveMap" + map);
                    SetActiveMap(map);
                    GameManager.NotificationSystem.mapLoaded.Invoke();
                    onCompletion?.Invoke();
                };
            }
            else
            {
                GameManager.NotificationSystem.mapLoaded.Invoke();
                onCompletion?.Invoke();
            }
        }

        private void CompleteTransition(Action onCompletion)
        {
            GameManager.NotificationSystem.mapTransitionCompleted.Invoke();
             
            // the "?." similar to if (onCompletion != null) onCompletion.Invoke();
            onCompletion?.Invoke();
        }
    }
}
