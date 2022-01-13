using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yontalane
{
    [Serializable]
    public class SerializableKeyValuePair<T, U>
    {
        public T key;
        public U value;
    }

    [Serializable]
    public class SerializableDictionaryBase_DoNotUse { }

    [Serializable]
    public class SerializableDictionary<T, U> : SerializableDictionaryBase_DoNotUse, IEnumerator, IEnumerable
    {
        [SerializeField] private List<T> m_keys = new List<T>();
        [SerializeField] private List<U> m_values = new List<U>();

        #region Get

        /// <summary>
        /// Get the key at the provided index.
        /// </summary>
        public T GetKeyAt(int index)
        {
            FixSize();
            if (index >= 0 && index < Count) return m_keys[index];
            return default;
        }

        /// <summary>
        /// Get the value at the provided index.
        /// </summary>
        public U GetValueAt(int index)
        {
            FixSize();
            if (index >= 0 && index < Count) return m_values[index];
            return default;
        }

        /// <summary>
        /// Get the key/value pair at the provided index.
        /// </summary>
        public SerializableKeyValuePair<T, U> GetAt(int index)
        {
            FixSize();
            if (index >= 0 && index < Count) return new SerializableKeyValuePair<T, U>()
            {
                key = m_keys[index],
                value = m_values[index]
            };
            return default;
        }

        /// <summary>
        /// If the key exists, set <c>value</c> to its associated value and return true. Otherwise, return false.
        /// </summary>
        public bool TryGet(T key, out U value)
        {
            FixSize();
            for (int i = 0; i < m_keys.Count; i++)
            {
                if (m_keys[i].Equals(key))
                {
                    value = m_values[i];
                    return true;
                }
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Return the value for the key.
        /// </summary>
        public U Get(T key) => TryGet(key, out U value) ? value : default;

        #endregion

        #region Add

        /// <summary>
        /// Add a key/value pair.
        /// </summary>
        public void Add(T key, U value)
        {
            FixSize();
            m_keys.Add(key);
            m_values.Add(value);
        }

        /// <summary>
        /// Add a key/value pair.
        /// </summary>
        public void Add(SerializableKeyValuePair<T, U> pair)
        {
            FixSize();
            m_keys.Add(pair.key);
            m_values.Add(pair.value);
        }

        /// <summary>
        /// Insert a key/value pair at the provided index.
        /// </summary>
        public void Insert(int index, T key, U value)
        {
            FixSize();
            m_keys.Insert(index, key);
            m_values.Insert(index, value);
        }

        /// <summary>
        /// Insert a key/value pair at the provided index.
        /// </summary>
        public void Insert(int index, SerializableKeyValuePair<T, U> pair)
        {
            FixSize();
            m_keys.Insert(index, pair.key);
            m_values.Insert(index, pair.value);
        }

        #endregion

        #region Remove

        /// <summary>
        /// Remove the provided key and its associated value.
        /// </summary>
        public void Remove(T key)
        {
            for (int i = m_keys.Count - 1; i >= 0; i--)
            {
                if (m_keys[i].Equals(key))
                {
                    m_keys.RemoveAt(i);
                    m_values.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Remove the provided value and its associated key.
        /// </summary>
        public void Remove(U value)
        {
            for (int i = m_values.Count - 1; i >= 0; i--)
            {
                if (m_values[i].Equals(value))
                {
                    m_keys.RemoveAt(i);
                    m_values.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Remove the key/value pair at the provided index.
        /// </summary>
        public void RemoveAt(int index)
        {
            FixSize();
            if (index >= 0 && index < m_keys.Count)
            {
                m_keys.RemoveAt(index);
                m_values.RemoveAt(index);
            }
        }

        /// <summary>
        /// Clear the dictionary.
        /// </summary>
        public void Clear()
        {
            m_keys.Clear();
            m_values.Clear();
        }

        #endregion

        #region Set

        /// <summary>
        /// Set the value associated with the provided key.
        /// </summary>
        public void Set(T key, U value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (m_keys[i].Equals(key)) m_values[i] = value;
            }
        }

        /// <summary>
        /// Set the key/value pair at the provided index.
        /// </summary>
        public void SetAt(int index, SerializableKeyValuePair<T, U> pair)
        {
            FixSize();
            if (index >= 0 && index < Count)
            {
                m_keys[index] = pair.key;
                m_values[index] = pair.value;
            }
            else if (index == Count) Add(pair);
        }

        #endregion

        /// <summary>
        /// Return true if the dictionary contains the provided key. Otherwise, return false.
        /// </summary>
        public bool Contains(T key)
        {
            FixSize();
            for (int i = 0; i < m_keys.Count; i++)
            {
                if (m_keys[i].Equals(key)) return true;
            }
            return false;
        }

        /// <summary>
        /// The number of key/value pairs contained within the dictionary.
        /// </summary>
        public int Count => Mathf.Min(m_keys.Count, m_values.Count);

        #region Utils

        private void FixSize()
        {
            int count = Count;
            while (m_keys.Count > count)
            {
                m_keys.RemoveAt(m_keys.Count - 1);
            }
            while (m_values.Count > count)
            {
                m_values.RemoveAt(m_values.Count - 1);
            }
        }

        #endregion

        #region Enumerator / Loop Iteration

        public IEnumerator GetEnumerator() => this;

        private int m_position = -1;

        public object Current => new SerializableKeyValuePair<T, U>()
        {
            key = m_keys[m_position],
            value = m_values[m_position]
        };

        public bool MoveNext()
        {
            m_position++;
            return m_position < Count;
        }

        public void Reset() => m_position = 0;

        #endregion
    }
}