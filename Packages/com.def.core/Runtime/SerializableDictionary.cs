using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DEF
{
    [Serializable]
    public class SerializableKeyValuePair<T, U>
    {
        public T key;
        public U value;
    }

    [Serializable]
    public class SerializableDictionary<T, U> : IEnumerator, IEnumerable
    {
        [SerializeField] private List<T> m_keys = new List<T>();
        [SerializeField] private List<U> m_values = new List<U>();

        #region get
        public T GetKeyAt(int index)
        {
            if (index >= 0 && index < Count) return m_keys[index];
            return default;
        }

        public U GetValueAt(int index)
        {
            if (index >= 0 && index < Count) return m_values[index];
            return default;
        }

        public SerializableKeyValuePair<T, U> GetAt(int index)
        {
            if (index >= 0 && index < Count) return new SerializableKeyValuePair<T, U>()
            {
                key = m_keys[index],
                value = m_values[index]
            };
            return default;
        }

        public bool TryGet(T key, out U value)
        {
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

        public U Get(T key) => TryGet(key, out U value) ? value : default;
        #endregion

        #region add
        public void Add(T key, U value)
        {
            m_keys.Add(key);
            m_values.Add(value);
        }

        public void Add(SerializableKeyValuePair<T, U> pair)
        {
            m_keys.Add(pair.key);
            m_values.Add(pair.value);
        }

        public void Insert(int index, SerializableKeyValuePair<T, U> pair)
        {
            m_keys.Insert(index, pair.key);
            m_values.Insert(index, pair.value);
        }

        public void Insert(int index, T key, U value)
        {
            m_keys.Insert(index, key);
            m_values.Insert(index, value);
        }
        #endregion

        #region remove
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

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < m_keys.Count)
            {
                m_keys.RemoveAt(index);
                m_values.RemoveAt(index);
            }
        }

        public void Clear()
        {
            m_keys.Clear();
            m_values.Clear();
        }
        #endregion

        #region set
        public void Set(T key, U value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (m_keys[i].Equals(key)) m_values[i] = value;
            }
        }

        public void SetAt(int index, SerializableKeyValuePair<T, U> pair)
        {
            if (index >= 0 && index < Count)
            {
                m_keys[index] = pair.key;
                m_values[index] = pair.value;
            }
            else if (index == Count) Add(pair);
        }
        #endregion

        public bool Contains(T key)
        {
            for (int i = 0; i < m_keys.Count; i++)
            {
                if (m_keys[i].Equals(key)) return true;
            }
            return false;
        }

        public int Count => Mathf.Min(m_keys.Count, m_values.Count);

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