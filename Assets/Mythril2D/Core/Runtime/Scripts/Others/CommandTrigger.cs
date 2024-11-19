using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class CommandTrigger : Entity
    {
        public enum EActivationEvent
        {
            OnStart,
            OnEnable,
            OnDisable,
            OnDestroy,
            OnUpdate,
            OnPlayerEnterTrigger,
            OnPlayerExitTrigger,
            OnPlayerInteract,
        }

        [Header("Requirements")]
        [SerializeField] private EActivationEvent m_activationEvent;
        [SerializeReference, SubclassSelector] private ICondition m_condition;

        [Header("Actions")]
        [SerializeReference, SubclassSelector] private ICommand m_toExecute;
        [SerializeReference, SubclassSelector] private ICommand[] m_toExecutes;

        [Header("Settings")]
        [SerializeField] private int m_frameDelay = 0;

        protected override void Start()
        {
            base.Start();
        }

        public void AttemptExecution(EActivationEvent currentEvent, GameObject go = null)
        {
            if (currentEvent == m_activationEvent && (go == null || go == GameManager.Player.gameObject) && (m_condition?.Evaluate() ?? true))
            {
                if (m_frameDelay <= 0)
                {
                    Execute();
                }
                else
                {
                    StartCoroutine(CoroutineHelpers.ExecuteInXFrames(m_frameDelay, Execute));
                }
            }
        }

        private void Execute()
        {
            if(m_toExecutes != null)
            {
                foreach (var toExecute in m_toExecutes)
                {
                    toExecute?.Execute();
                }
            }
            else
            {
                m_toExecute?.Execute();
            }

        }

        //private void Start() => AttemptExecution(EActivationEvent.OnStart);

        private void Update() => AttemptExecution(EActivationEvent.OnUpdate);
        private void OnEnable() => AttemptExecution(EActivationEvent.OnEnable);
        private void OnDisable() => AttemptExecution(EActivationEvent.OnDisable);
        private void OnDestroy() => AttemptExecution(EActivationEvent.OnDestroy);
        private void OnTriggerEnter2D(Collider2D collider) => AttemptExecution(EActivationEvent.OnPlayerEnterTrigger, collider.gameObject);
        private void OnTriggerExit2D(Collider2D collider) => AttemptExecution(EActivationEvent.OnPlayerExitTrigger, collider.gameObject);
        private void OnInteract(CharacterBase sender) => AttemptExecution(EActivationEvent.OnPlayerInteract, sender.gameObject);
    }
}