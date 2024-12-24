using UnityEngine;
using Gyvr.Mythril2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Windows;

public class Trap : MonoBehaviour
{
    [SerializeField] private int m_damageAmount = 1;
    [SerializeField] private EDamageType m_damageType = EDamageType.Physical;
    [SerializeField] private EDistanceType m_distanceType = EDistanceType.Ranged;
    // Time in seconds between each damage application
    [SerializeField] private float m_damageInterval = 1.0f;

    // Track colliders in trigger
    private HashSet<Collider2D> m_collidersInTrigger = new HashSet<Collider2D>();
    private Coroutine m_damageCoroutine;

    private void OnDisable()
    {
        //Debug.Log(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var character = other.gameObject.GetComponent<CharacterBase>();


        if (character != null)
        {
            m_collidersInTrigger.Add(other);

            // Start the damage coroutine if it's not already running
            if (m_damageCoroutine == null)
            {
                m_damageCoroutine = StartCoroutine(ApplyDamage());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var character = other.gameObject.GetComponent<CharacterBase>();

        if (character != null)
        {
            m_collidersInTrigger.Remove(other);

            // Stop the coroutine if no colliders are left in the trigger
            if (m_collidersInTrigger.Count == 0)
            {
                if (m_damageCoroutine != null)
                {
                    StopCoroutine(m_damageCoroutine);
                    m_damageCoroutine = null;
                }
            }
        }
    }

    private IEnumerator ApplyDamage()
    {
        while (true)
        {
            // 创建一个临时集合的快照 防止直接修改集合报错
            var collidersSnapshot = new List<Collider2D>(m_collidersInTrigger);

            foreach (var collider in collidersSnapshot)
            {
                var character = collider.gameObject.GetComponent<CharacterBase>();

                if (character != null && character.isPlayer == true)
                {
                    character.Damage(new DamageOutputDescriptor
                    {
                        source = EDamageSource.Unknown,
                        attacker = this,
                        damage = m_damageAmount,
                        damageType = m_damageType,
                        distanceType = m_distanceType,
                        flags = EDamageFlag.None
                    });
                }
            }

            yield return new WaitForSeconds(m_damageInterval);
        }
    }
}