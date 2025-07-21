using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// A serializable key/value pair.
    /// </summary>
    [Serializable]
    public class SerializableKeyValuePair<T, U>
    {
        public T key;
        public U value;
    }

    /// <summary>
    /// A base class for serializable dictionaries.
    /// </summary>
    [Serializable]
    public class SerializableDictionaryBase_DoNotUse { }

    /// <summary>
    /// A serializable dictionary.
    /// </summary>
    [Serializable]
    public class SerializableDictionary<T, U> : SerializableDictionaryBase_DoNotUse, IEnumerator, IEnumerable
    {
        [SerializeField] private List<T> m_keys = new List<T>();
        [SerializeField] private List<U> m_values = new List<U>();

        #region Get

        /// <summary>
        /// Gets the key at the provided index.
        /// </summary>
        public T GetKeyAt(int index)
        {
            FixSize();
            if (index >= 0 && index < Count) return m_keys[index];
            return default;
        }

        /// <summary>
        /// Gets the value at the provided index.
        /// </summary>
        public U GetValueAt(int index)
        {
            FixSize();
            if (index >= 0 && index < Count) return m_values[index];
            return default;
        }

        /// <summary>
        /// Gets the key/value pair at the provided index.
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
        /// If the key exists, sets <c>value</c> to its associated value and returns true. Otherwise, returns false.
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
        /// Returns the value for the key.
        /// </summary>
        public U Get(T key) => TryGet(key, out U value) ? value : default;

        #endregion

        #region Add

        /// <summary>
        /// Adds a key/value pair.
        /// </summary>
        public void Add(T key, U value)
        {
            FixSize();
            m_keys.Add(key);
            m_values.Add(value);
        }

        /// <summary>
        /// Adds a key/value pair.
        /// </summary>
        public void Add(SerializableKeyValuePair<T, U> pair)
        {
            FixSize();
            m_keys.Add(pair.key);
            m_values.Add(pair.value);
        }

        /// <summary>
        /// Inserts a key/value pair at the provided index.
        /// </summary>
        public void Insert(int index, T key, U value)
        {
            FixSize();
            m_keys.Insert(index, key);
            m_values.Insert(index, value);
        }

        /// <summary>
        /// Inserts a key/value pair at the provided index.
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
        /// Removes the provided key and its associated value.
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
        /// Removes the key/value pair at the provided index.
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
        /// Clears the dictionary.
        /// </summary>
        public void Clear()
        {
            m_keys.Clear();
            m_values.Clear();
        }

        #endregion

        #region Set

        /// <summary>
        /// Sets the value associated with the provided key.
        /// </summary>
        public void Set(T key, U value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (m_keys[i].Equals(key)) m_values[i] = value;
            }
        }

        /// <summary>
        /// Sets the key/value pair at the provided index.
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
        /// If the dictionary contains the provided key, returns its index; otherwise, returns -1.
        /// </summary>
        public int IndexOf(T key)
        {
            FixSize();
            for (int i = 0; i < m_keys.Count; i++)
            {
                if (m_keys[i].Equals(key)) return i;
            }
            return -1;
        }

        /// <summary>
        /// Returns true if the dictionary contains the provided key. Otherwise, returns false.
        /// </summary>
        public bool Contains(T key) => IndexOf(key) != -1;

        /// <summary>
        /// Gets the number of key/value pairs contained within the dictionary.
        /// </summary>
        public int Count => Mathf.Min(m_keys.Count, m_values.Count);

        #region Utils

        /// <summary>
        /// Fixes the size of the dictionary.
        /// </summary>
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

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        public IEnumerator GetEnumerator() => this;

        /// <summary>
        /// The current position of the enumerator.
        /// </summary>
        private int m_position = -1;

        /// <summary>
        /// Gets the current key/value pair.
        /// </summary>
        public object Current => new SerializableKeyValuePair<T, U>()
        {
            key = m_keys[m_position],
            value = m_values[m_position]
        };

        /// <summary>
        /// Moves to the next key/value pair.
        /// </summary>
        public bool MoveNext()
        {
            m_position++;
            return m_position < Count;
        }

        /// <summary>
        /// Resets the enumerator to the first key/value pair.
        /// </summary>
        public void Reset() => m_position = 0;

        #endregion
    }
}