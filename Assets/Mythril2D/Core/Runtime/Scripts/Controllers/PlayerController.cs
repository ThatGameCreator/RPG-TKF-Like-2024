using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gyvr.Mythril2D
{
    [RequireComponent(typeof(CharacterBase))]
    public class PlayerController : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private Transform m_overrideInteractionPivot = null;
        [SerializeField] private float m_interactionDistance = 0.75f;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_interactionSound;

        [Header("Dash")]
        [SerializeField][Min(0)] private float m_dashStrength = 1;
        [SerializeField][Min(0)] private float m_dashResistance = 1;
        [SerializeField] private ParticleSystem m_dashParticleSystem = null;
        private Vector2 m_dirction = Vector2.zero;

        public Entity interactionTarget => m_interactionTarget;

        private Entity m_interactionTarget = null;

        private CharacterBase m_character = null;

        private Vector2 m_movementDirection;
        private Transform m_interactionPivot = null;


        private void Awake()
        {
            m_character = GetComponent<CharacterBase>();

            if (m_overrideInteractionPivot)
            {
                m_interactionPivot = m_overrideInteractionPivot;
            }
            else
            {
                m_interactionPivot = transform;
            }

            m_movementDirection = Vector2.right;
        }

        private void Start()
        {
            GameManager.InputSystem.gameplay.interact.performed += OnInputInteract;
            GameManager.InputSystem.gameplay.fireAbility1.performed += OnFireAbility1;
            GameManager.InputSystem.gameplay.fireAbility2.performed += OnFireAbility2;
            GameManager.InputSystem.gameplay.fireAbility3.performed += OnFireAbility3;
            GameManager.InputSystem.gameplay.fireAbility4.performed += OnFireAbility4;
            GameManager.InputSystem.gameplay.move.performed += OnMove;
            GameManager.InputSystem.gameplay.move.canceled += OnStoppedMoving;
            GameManager.InputSystem.gameplay.run.performed += OnRun;
            GameManager.InputSystem.gameplay.run.canceled += OnStoppedRunning;
            GameManager.InputSystem.gameplay.openGameMenu.performed += OnOpenGameMenu;
            GameManager.InputSystem.gameplay.dash.performed += OnDashAbility;
        }

        private void OnDestroy()
        {
            GameManager.InputSystem.gameplay.interact.performed -= OnInputInteract;
            GameManager.InputSystem.gameplay.fireAbility1.performed -= OnFireAbility1;
            GameManager.InputSystem.gameplay.fireAbility2.performed -= OnFireAbility2;
            GameManager.InputSystem.gameplay.fireAbility3.performed -= OnFireAbility3;
            GameManager.InputSystem.gameplay.fireAbility4.performed -= OnFireAbility4;
            GameManager.InputSystem.gameplay.move.performed -= OnMove;
            GameManager.InputSystem.gameplay.move.canceled -= OnStoppedMoving;
            GameManager.InputSystem.gameplay.run.performed -= OnRun;
            GameManager.InputSystem.gameplay.run.canceled += OnStoppedRunning;
            GameManager.InputSystem.gameplay.openGameMenu.performed -= OnOpenGameMenu;
            GameManager.InputSystem.gameplay.dash.performed -= OnDashAbility;
        }

        private void Update()
        {
            // �������ͣ��ȡ��Ϊ���ܹ��ڽӽ������ʱ��ᵯ��һ���ɻ����İ�ť
            m_interactionTarget = GetInteractibleObject();
        }

        public Entity GetInteractibleObject()
        {
            if (m_character.Can(EActionFlags.Interact))
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(m_interactionPivot.position, m_interactionDistance, 
                    LayerMask.GetMask(GameManager.Config.interactionLayer));
                
                Array.Sort(colliders, (x, y) =>
                {
                    return Vector3.Distance(m_interactionPivot.position, x.transform.position).CompareTo(
                        Vector3.Distance(m_interactionPivot.position, y.transform.position));
                });
                
                foreach (Collider2D collider in colliders)
                {
                    Vector3 a = m_movementDirection;
                    Vector3 b = (collider.transform.position + new Vector3(collider.offset.x, collider.offset.y, 0)) - m_interactionPivot.position;

                    if (Vector3.Dot(a, b) > 0)
                    {
                        Entity entity = collider.GetComponent<Entity>();
                        if (entity != null)
                        {
                            return entity;
                        }
                    }
                }
            }

            return null;
        }

        private bool TryInteracting()
        {
            Entity interactionTarget = GetInteractibleObject();

            if (interactionTarget)
            {
                ActiveInteracting();
                return true;
            }

            return false;
        }

        private void ActiveInteracting()
        {
            // �о������������������Ч�Ļ���û��Ҫ���������Ч
            //GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_interactionSound);

            GameManager.NotificationSystem.playerTryInteracte.Invoke(GameManager.Player, interactionTarget);
        }

        private void OnOpenGameMenu(InputAction.CallbackContext context)
        {
            GameManager.Player.CancelLooting();

            GameManager.NotificationSystem.gameMenuRequested.Invoke();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            Vector2 direction = context.ReadValue<Vector2>();

            m_character.SetMovementDirection(direction);

            if (direction.magnitude > 0.01f)
            {
                m_movementDirection = direction;
            }
        }

        private void OnStoppedMoving(InputAction.CallbackContext context)
        {
            m_character.SetMovementDirection(Vector2.zero);
        }

        private void OnRun(InputAction.CallbackContext context)
        {
            if (context.performed == true)
            {
                //Debug.Log("OnRun");

                if (GameManager.Player.currentStamina > 0)
                {
                    m_character.TryPlayRunAnimation();
                }
            }
        }

        private void OnStoppedRunning(InputAction.CallbackContext context)
        {
            if (context.performed == false)
            {
                m_character.EndPlayRunAnimation();
            }
        }

        // û���ڽ�ɫ�й���Dash����ʱ ֻ��������ִ�з���
        private void OnDash(InputAction.CallbackContext context)
        {
            m_dirction =
                    m_character.IsMoving() ?
                    m_character.movementDirection :
                    (m_character.GetLookAtDirection() == EDirection.Right ? Vector2.right : Vector2.left);

            if (IsPlayingDashAnimation() == false)
            {
                //Debug.Log("Playing Dash Animation false");
            }

            m_character.Push(m_dirction, m_dashStrength, m_dashResistance, faceOppositeDirection: true);

            if (m_dashParticleSystem != null)
            {
                m_dashParticleSystem.Play();
            }

            //m_character.DashConsumeStamina();

            m_character.TryPlayDashAnimation();
        }
        // Line break so it looks nicer :)

        bool IsPlayingDashAnimation()
        {
            AnimatorStateInfo stateInfo = m_character.animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName("Dash") == true) return true;
            else return false;
        }

        private void OnInputInteract(InputAction.CallbackContext context) =>  TryInteracting();

        private void OnFireAbility1(InputAction.CallbackContext context) => FireAbilityAtIndex(0);
        private void OnFireAbility2(InputAction.CallbackContext context) => FireAbilityAtIndex(1);
        private void OnFireAbility3(InputAction.CallbackContext context) => FireAbilityAtIndex(2);
        private void OnFireAbility4(InputAction.CallbackContext context) => FireAbilityAtIndex(3);
        private void OnDashAbility(InputAction.CallbackContext context) => GetDashAbility();

        private void GetDashAbility()
        {
            DashAbilitySheet selectedAbility = GameManager.Player.dashAbility;

            if (selectedAbility != null)
            {
                m_character.FireAbility(selectedAbility);
            }
        }

        private void FireAbilityAtIndex(int i)
        {
            AbilitySheet selectedAbility = GetAbilityAtIndex(i);

            if (selectedAbility != null)
            {
                m_character.FireAbility(selectedAbility);
            }
        }
        
        protected AbilitySheet GetAbilityAtIndex(int index)
        {
            return GameManager.Player.equippedAbilities[index];
        }
    }
}
