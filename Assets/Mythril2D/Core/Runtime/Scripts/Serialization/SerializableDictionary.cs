// From GitHub: https://github.com/azixMcAze/Unity-SerializableDictionary

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace Gyvr.Mythril2D
{

    public abstract class SerializableDictionaryBase
    {
        public abstract class Storage { }

        protected class Dictionary<TKey, TValue> : System.Collections.Generic.Dictionary<TKey, TValue>
        {
            public Dictionary() { }
            public Dictionary(IDictionary<TKey, TValue> dict) : base(dict) { }
            public Dictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
    }

    [Serializable]
    public abstract class SerializableDictionaryBase<TKey, TValue, TValueStorage> : SerializableDictionaryBase, IDictionary<TKey, TValue>, IDictionary, ISerializationCallbackReceiver, IDeserializationCallback, ISerializable
    {
        //[SerializeField]
        //TKey[] m_keys;
        //[SerializeField]
        //TValueStorage[] m_values;

        //Dictionary<TKey, TValue> m_dict;

        [SerializeField]
        //private List<KeyValuePair<TKey, TValue>> m_keyValuePairs = new List<KeyValuePair<TKey, TValue>>();
        private Dictionary<TKey, TValue> m_dict = new Dictionary<TKey, TValue>(); // 核心字典数据

        [SerializeField]
        private List<TKey> m_keys = new List<TKey>(); // 用于序列化的键列表
        [SerializeField]
        private List<TValue> m_values = new List<TValue>(); // 用于序列化的值列表

        // 同步键值列表回字典
        private void SyncListsToDictionary()
        {
            m_dict.Clear();

            if (m_keys.Count != m_values.Count)
            {
                Debug.LogError("Key and value list lengths do not match. Cannot deserialize dictionary.");
                return;
            }

            for (int i = 0; i < m_keys.Count; i++)
            {
                if (m_keys[i] == null)
                {
                    Debug.LogWarning($"Skipping null key at index {i}."); // 改为警告
                    continue;
                }

                if (!m_dict.ContainsKey(m_keys[i]))
                {
                    m_dict[m_keys[i]] = m_values[i];
                }
                else
                {
                    Debug.LogWarning($"Duplicate key detected: {m_keys[i]}");
                }
            }
        }

        // 在序列化前同步数据
        public void OnBeforeSerialize()
        {
            m_keys.Clear();
            m_values.Clear();

            foreach (var kvp in m_dict)
            {
                if (kvp.Key != null)  // 确认键不为 null
                {
                    m_keys.Add(kvp.Key);
                    m_values.Add(kvp.Value);
                }
                else
                {
                    Debug.LogWarning("Skipping null key during serialization.");
                }
            }
        }

        // 在反序列化后同步数据
        public void OnAfterDeserialize()
        {
            if (m_keys == null) m_keys = new List<TKey>();
            if (m_values == null) m_values = new List<TValue>();

            //Debug.Log($"Deserializing: m_keys count = {m_keys.Count}, m_values count = {m_values.Count}");

            SyncListsToDictionary();
        }


        //public void OnAfterDeserialize()
        //{
        //    if (m_keys != null && m_values != null && m_keys.Length == m_values.Length)
        //    {
        //        m_dict.Clear();
        //        int n = m_keys.Length;
        //        for (int i = 0; i < n; ++i)
        //        {
        //            m_dict[m_keys[i]] = GetValue(m_values, i);
        //        }

        //        m_keys = null;
        //        m_values = null;
        //    }
        //}

        //public void OnBeforeSerialize()
        //{
        //    int n = m_dict.Count;
        //    m_keys = new TKey[n];
        //    m_values = new TValueStorage[n];

        //    int i = 0;
        //    foreach (var kvp in m_dict)
        //    {
        //        m_keys[i] = kvp.Key;
        //        SetValue(m_values, i, kvp.Value);
        //        ++i;
        //    }
        //}

        public SerializableDictionaryBase()
        {
            m_dict = new Dictionary<TKey, TValue>();
        }

        public SerializableDictionaryBase(IDictionary<TKey, TValue> dict)
        {
            m_dict = new Dictionary<TKey, TValue>(dict);
        }

        protected abstract void SetValue(TValueStorage[] storage, int i, TValue value);
        protected abstract TValue GetValue(TValueStorage[] storage, int i);

        public void CopyFrom(IDictionary<TKey, TValue> dict)
        {
            m_dict.Clear();
            foreach (var kvp in dict)
            {
                m_dict[kvp.Key] = kvp.Value;
            }
        }

        #region IDictionary<TKey, TValue>

        //public ICollection<TKey> Keys { get { return ((IDictionary<TKey, TValue>)m_dict).Keys; } }
        //public ICollection<TValue> Values { get { return ((IDictionary<TKey, TValue>)m_dict).Values; } }
        //public int Count { get { return ((IDictionary<TKey, TValue>)m_dict).Count; } }
        //public bool IsReadOnly { get { return ((IDictionary<TKey, TValue>)m_dict).IsReadOnly; } }

        //public TValue this[TKey key]
        //{
        //    get { return ((IDictionary<TKey, TValue>)m_dict)[key]; }
        //    set { ((IDictionary<TKey, TValue>)m_dict)[key] = value; }
        //}

        public ICollection<TKey> Keys => m_dict.Keys;
        public ICollection<TValue> Values => m_dict.Values;
        public int Count => m_dict.Count;
        public bool IsReadOnly => false;

        // 字典核心操作
        public TValue this[TKey key]
        {
            get => m_dict[key];
            set => m_dict[key] = value;
        }

        //public void Add(TKey key, TValue value)
        //{
        //    ((IDictionary<TKey, TValue>)m_dict).Add(key, value);
        //}

        //public bool ContainsKey(TKey key)
        //{
        //    return ((IDictionary<TKey, TValue>)m_dict).ContainsKey(key);
        //}

        //public bool Remove(TKey key)
        //{
        //    return ((IDictionary<TKey, TValue>)m_dict).Remove(key);
        //}

        //public bool TryGetValue(TKey key, out TValue value)
        //{
        //    return ((IDictionary<TKey, TValue>)m_dict).TryGetValue(key, out value);
        //}

        public void Add(TKey key, TValue value) => m_dict.Add(key, value);
        public bool ContainsKey(TKey key) => m_dict.ContainsKey(key);
        public bool Remove(TKey key) => m_dict.Remove(key);
        public bool TryGetValue(TKey key, out TValue value) => m_dict.TryGetValue(key, out value);


        //public void Add(KeyValuePair<TKey, TValue> item)
        //{
        //    ((IDictionary<TKey, TValue>)m_dict).Add(item);
        //}

        //public void Clear()
        //{
        //    ((IDictionary<TKey, TValue>)m_dict).Clear();
        //}

        //public bool Contains(KeyValuePair<TKey, TValue> item)
        //{
        //    return ((IDictionary<TKey, TValue>)m_dict).Contains(item);
        //}

        //public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        //{
        //    ((IDictionary<TKey, TValue>)m_dict).CopyTo(array, arrayIndex);
        //}

        //public bool Remove(KeyValuePair<TKey, TValue> item)
        //{
        //    return ((IDictionary<TKey, TValue>)m_dict).Remove(item);
        //}

        //public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        //{
        //    return ((IDictionary<TKey, TValue>)m_dict).GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return ((IDictionary<TKey, TValue>)m_dict).GetEnumerator();
        //}

        public void Add(KeyValuePair<TKey, TValue> item) => m_dict.Add(item.Key, item.Value);
        public void Clear() => m_dict.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => m_dict.Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)m_dict).CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<TKey, TValue> item) => m_dict.Remove(item.Key);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => m_dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => m_dict.GetEnumerator();


        #endregion

        #region IDictionary

        public bool IsFixedSize { get { return ((IDictionary)m_dict).IsFixedSize; } }
        ICollection IDictionary.Keys { get { return ((IDictionary)m_dict).Keys; } }
        ICollection IDictionary.Values { get { return ((IDictionary)m_dict).Values; } }
        public bool IsSynchronized { get { return ((IDictionary)m_dict).IsSynchronized; } }
        public object SyncRoot { get { return ((IDictionary)m_dict).SyncRoot; } }

        public object this[object key]
        {
            get { return ((IDictionary)m_dict)[key]; }
            set { ((IDictionary)m_dict)[key] = value; }
        }

        public void Add(object key, object value)
        {
            ((IDictionary)m_dict).Add(key, value);
        }

        public bool Contains(object key)
        {
            return ((IDictionary)m_dict).Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)m_dict).GetEnumerator();
        }

        public void Remove(object key)
        {
            ((IDictionary)m_dict).Remove(key);
        }

        public void CopyTo(Array array, int index)
        {
            ((IDictionary)m_dict).CopyTo(array, index);
        }

        #endregion

        #region IDeserializationCallback

        public void OnDeserialization(object sender)
        {
            ((IDeserializationCallback)m_dict).OnDeserialization(sender);
        }

        #endregion

        #region ISerializable

        //protected SerializableDictionaryBase(SerializationInfo info, StreamingContext context)
        //{
        //    m_dict = new Dictionary<TKey, TValue>(info, context);
        //}

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var kvp in m_dict)
            {
                string keyString = kvp.Key.ToString(); // 假设 TKey 实现了 ToString()
                info.AddValue(keyString, kvp.Value);
            }
        }
        protected SerializableDictionaryBase(SerializationInfo info, StreamingContext context)
        {
            m_dict = new Dictionary<TKey, TValue>();
            foreach (SerializationEntry entry in info)
            {
                try
                {
                    TKey key = (TKey)Convert.ChangeType(entry.Name, typeof(TKey)); // 使用 Convert.ChangeType 转换
                    TValue value = (TValue)entry.Value;
                    m_dict.Add(key, value);
                }
                catch (InvalidCastException e)
                {
                    Debug.LogError($"Failed to deserialize key '{entry.Name}': {e.Message}");
                }
            }
        }

        #endregion
    }

    public static class SerializableDictionary
    {
        public class Storage<T> : SerializableDictionaryBase.Storage
        {
            public T data;
        }
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase<TKey, TValue, TValue>
    {

        public SerializableDictionary() : base() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base()
        {
            foreach (var kvp in dictionary)
            {
                Add(kvp.Key, kvp.Value);
            }
        }

        //public SerializableDictionary() { }
        //public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict) { }


        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        protected override TValue GetValue(TValue[] storage, int i)
        {
            return storage[i];
        }

        protected override void SetValue(TValue[] storage, int i, TValue value)
        {
            storage[i] = value;
        }
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue, TValueStorage> : SerializableDictionaryBase<TKey, TValue, TValueStorage> where TValueStorage : SerializableDictionary.Storage<TValue>, new()
    {
        public SerializableDictionary() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict) { }
        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        protected override TValue GetValue(TValueStorage[] storage, int i)
        {
            return storage[i].data;
        }

        protected override void SetValue(TValueStorage[] storage, int i, TValue value)
        {
            storage[i] = new TValueStorage();
            storage[i].data = value;
        }
    }

}
