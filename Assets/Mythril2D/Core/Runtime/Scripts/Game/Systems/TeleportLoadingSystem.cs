using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gyvr.Mythril2D
{
    public class TeleportLoadingDelegationParams
    {
        public Action<Action> unloadDelegate;
        public Action<Action> loadDelegate;
        public Action completionDelegate;
    }

    public enum ETeleportType
    {
        None,
        Normal,    
        Revival,
    }

    public class TeleportLoadingSystem : AGameSystem
    {
        [Header("Settings")]
        // 如果委托传送，则会先执行存档在执行传送
        [SerializeField] private bool m_delegateTransitionResponsability = false;
        [SerializeField] private GameObject m_defaultResurrectionPostion = null;
        [SerializeField] private string m_defaultResurrectionMap = null;

        private string m_currentMap = string.Empty;
        private Vector2 m_currentRevivalPostion = new Vector2();

        public void SetSaveResurrectionPostion(Vector2 postion)
        {
            m_currentRevivalPostion = postion;
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

        public void RequestTransition(string map = null, Action onMapUnloaded = null, Action onMapLoaded = null, Action onCompletion = null, 
            ETeleportType eTeleportType = ETeleportType.None, string destinationGameObjectName = null)
        {
            if(map == null)
            {
                map = m_defaultResurrectionMap;
            }

            // 传送地图也要设置当前地图
            // 好像在这里设置会出问题
            //SetActiveMap(map);

            onCompletion += () =>
            {
                if (eTeleportType != ETeleportType.None)
                {
                    Debug.Log(String.Format("TryExcutePositionTelepot"));
                    TryExcutePositionTelepot(eTeleportType, destinationGameObjectName);
                }
            };

            GameManager.NotificationSystem.mapTransitionStarted.Invoke();

            if (map != GameManager.TeleportLoadingSystem.GetCurrentMapName())
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
        }

        private void TryExcutePositionTelepot(ETeleportType eTeleportType, string destinationGameObjectName) {

            if (eTeleportType == ETeleportType.Revival)
            {
                RevivalTeloportPlayerPosition();

                // 不调用这个方法会设全部 Input Action 为 False
                // 地图传送后也会调用一次，应该加点判断啥的
                GameManager.NotificationSystem.mapTransitionCompleted.Invoke();
            }
            else if (eTeleportType == ETeleportType.Normal)
            {
                Debug.Log(String.Format("ETeleportType.Normal"));

                if (destinationGameObjectName == null)
                {
                    Debug.LogWarning("Destination GameObject Name is Null.");
                }
                else
                {
                    Debug.Log(String.Format("TeloportPlayerPosition"));

                    TeloportPlayerPosition(destinationGameObjectName);
                }
            }
        }

        protected void RevivalTeloportPlayerPosition()
        {
            if (m_currentRevivalPostion == null)
            {
                GameManager.Player.transform.position = m_defaultResurrectionPostion.transform.position;
            }
            else
            {
                GameManager.Player.transform.position = m_currentRevivalPostion;
            }
        }

        protected bool TeloportPlayerPosition(string m_destinationGameObjectName)
        {
            GameObject destionationGameObject = GameObject.Find(m_destinationGameObjectName);

            if (destionationGameObject)
            {
                Debug.Log(String.Format("GameObject.Find"));

                Debug.Log(String.Format("PlayerPositon, DestinationPosition = {0}, {1}", GameManager.Player.transform.position, destionationGameObject.transform.position));

                GameManager.Player.transform.position = destionationGameObject.transform.position;
            }

            // end of teleport
            return false;
        }

        private void DelegateTransition(string map, Action onMapUnloaded = null, Action onMapLoaded = null, Action onCompletion = null)
        {
            GameManager.NotificationSystem.mapTransitionDelegationRequested.Invoke(new TeleportLoadingDelegationParams
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
