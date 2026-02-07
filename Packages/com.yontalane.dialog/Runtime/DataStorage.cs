using System.Collections.Generic;
using UnityEngine;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Provides a static storage for dialog-related variables used by the DialogProcessor.
    /// </summary>
    [System.Serializable]
    public static class DataStorage
    {
        /// <summary>
        /// The serializable key-value pair used for data storage by the DialogProcessor.
        /// </summary>
        [System.Serializable]
        public struct DataStorageVar
        {
            /// <summary>
            /// The keyword for the data item.
            /// </summary>
            [Tooltip("The keyword for the data item")]
            public string key;

            /// <summary>
            /// The value of the data item.
            /// </summary>
            [Tooltip("The value of the data item.")]
            public string value;
        }

        private static DataStorageContainer s_storageContainer;
        
        /// <summary>
        /// Gets the number of elements contained in the storage list.
        /// </summary>
        public static int Count => Vars.Count;

        /// <summary>
        /// A static dictionary that the DialogProcessor can use to keep track of the dialog state.
        /// </summary>
        private static List<DataStorageVar> Vars
        {
            get
            {
                if (s_storageContainer == null)
                {
                    s_storageContainer = Object.FindAnyObjectByType<DataStorageContainer>();

                    if (s_storageContainer == null)
                    {
                        s_storageContainer = new  GameObject().AddComponent<DataStorageContainer>();
                        s_storageContainer.name = nameof(DataStorageContainer);
                        s_storageContainer.transform.position =  Vector3.zero;
                        s_storageContainer.transform.localEulerAngles = Vector3.zero;
                        s_storageContainer.transform.localScale = Vector3.one;
                        s_storageContainer.gameObject.isStatic = true;
                        Object.DontDestroyOnLoad(s_storageContainer);
                    }
                }

                return s_storageContainer.Vars;
            }
        }

        /// <summary>
        /// Get all keys as a list of strings.
        /// </summary>
        /// <param name="keys">The string list to populate with keys.</param>
        public static void GetAllKeys(List<string> keys)
        {
            if (keys == null)
            {
                Debug.LogWarning($"{nameof(GetAllKeys)} requires a non-null list parameter.");
                return;
            }

            keys.Clear();

            foreach (DataStorageVar var in Vars)
            {
                keys.Add(var.key);
            }
        }

        /// <summary>
        /// Get all pairs as a dictionary.
        /// </summary>
        /// <param name="pairs">The dictionary to populate with pairs.</param>
        public static void GetAllKeyValuePairs(Dictionary<string, string> pairs)
        {
            if (pairs == null)
            {
                Debug.LogWarning($"{nameof(GetAllKeys)} requires a non-null dictionary parameter.");
                return;
            }

            pairs.Clear();

            foreach (DataStorageVar var in Vars)
            {
                pairs.Add(var.key, var.value);
            }
        }

        /// <summary>
        /// Adds a DataStorageVar to the storage list.
        /// </summary>
        /// <param name="var">The variable to add.</param>
        public static void Add(DataStorageVar var) => Vars.Add(var);

        /// <summary>
        /// Adds a key-value pair to the storage list.
        /// </summary>
        /// <param name="key">The key of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        public static void Add(string key, string value) => Add(new()
        {
            key = key,
            value = value,
        });

        /// <summary>
        /// Checks if the storage contains a variable with the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public static bool ContainsKey(string key)
        {
            foreach (DataStorageVar var in Vars)
            {
                if (var.key == key)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to get the key-value pair at the specified index.
        /// </summary>
        /// <param name="index">The index of the key-value pair.</param>
        /// <param name="dataStorageVar">The key-value pair at the index, if found.</param>
        /// <returns>True if the key-value pair exists; otherwise, false.</returns>
        public static bool TryGetKeyValuePair(int index, out DataStorageVar dataStorageVar)
        {
            if (index < 0 || index >= Vars.Count)
            {
                dataStorageVar = default;
                return false;
            }
            
            dataStorageVar = Vars[index];
            return false;
        }

        /// <summary>
        /// Gets the key-value pair at the specified index.
        /// </summary>
        /// <param name="index">The index of the key-value pair.</param>
        /// <returns>The key-value pair.</returns>
        public static DataStorageVar GetKeyValuePair(int index)
        {
            return !TryGetKeyValuePair(index, out DataStorageVar dataStorageVar) ? default : dataStorageVar;
        }

        /// <summary>
        /// Attempts to get the key at the specified index.
        /// </summary>
        /// <param name="index">The index of the key-value pair.</param>
        /// <param name="key">The key at the index, if found.</param>
        /// <returns>True if the key-value pair exists; otherwise, false.</returns>
        public static bool TryGetKey(int index, out string key)
        {
            if (index < 0 || index >= Vars.Count)
            {
                key = default;
                return false;
            }
            
            key = Vars[index].key;
            return false;
        }

        /// <summary>
        /// Gets the key at the specified index.
        /// </summary>
        /// <param name="index">The index of the key-value pair.</param>
        /// <returns>The key at the index.</returns>
        public static string GetKey(int index)
        {
            return !TryGetKey(index, out string key) ? default : key;
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="value">The value associated with the key, if found.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public static bool TryGetValue(string key, out string value)
        {
            foreach (DataStorageVar var in Vars)
            {
                if (var.key == key)
                {
                    value = var.value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to get the value at the specified index.
        /// </summary>
        /// <param name="index">The index of the key-value pair.</param>
        /// <param name="value">The value at the index, if found.</param>
        /// <returns>True if the key-value pair exists; otherwise, false.</returns>
        public static bool TryGetValue(int index, out string value)
        {
            if (index < 0 || index >= Vars.Count)
            {
                value = default;
                return false;
            }
            
            value = Vars[index].value;
            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key, or returns a default value if the key does not exist.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="defaultValue">The value to return if the key does not exist.</param>
        /// <returns>The value associated with the key, or the default value.</returns>
        public static string GetValue(string key, string defaultValue = "")
        {
            return TryGetValue(key, out string result) ? result : defaultValue;
        }

        /// <summary>
        /// Gets the value at the specified index, or returns a default value if the key does not exist.
        /// </summary>
        /// <param name="index">The index of the key-value pair.</param>
        /// <param name="defaultValue">The value to return if the key does not exist.</param>
        /// <returns>The value at the index.</returns>
        public static string GetValue(int index, string defaultValue = "")
        {
            return TryGetValue(index, out string result) ? result : defaultValue;
        }

        /// <summary>
        /// Sets the value of a variable in the storage. If the key exists, updates its value; otherwise, adds it.
        /// </summary>
        /// <param name="var">The variable to set or add.</param>
        public static void SetValue(DataStorageVar var)
        {
            for (int i = 0; i < Vars.Count; i++)
            {
                if (Vars[i].key == var.key)
                {
                    Vars[i] = var;
                    return;
                }
            }

            Add(var);
        }

        /// <summary>
        /// Sets the value for a given key in the storage. If the key exists, updates its value; otherwise, adds it.
        /// </summary>
        /// <param name="key">The key of the variable.</param>
        /// <param name="value">The value to set.</param>
        public static void SetValue(string key, string value) => SetValue(new()
        {
            key = key,
            value = value,
        });

        /// <summary>
        /// Removes all elements from the storage list.
        /// </summary>
        public static void Clear() => Vars.Clear();

        #region Import/Export

        /// <summary>
        /// Exports the current storage variables to a JSON string.
        /// </summary>
        /// <returns>A JSON string representing the storage variables.</returns>
        public static string ExportToJson()
        {
            string json = JsonUtility.ToJson(Vars);
            return json;
        }

        /// <summary>
        /// Imports storage variables from a JSON string, replacing the current storage.
        /// </summary>
        /// <param name="json">A JSON string representing the storage variables.</param>
        public static void ImportFromJson(string json)
        {
            Vars.Clear();
            Vars.AddRange(JsonUtility.FromJson<List<DataStorageVar>>(json));
        }

        /// <summary>
        /// Exports the current storage variables to a text string.
        /// </summary>
        /// <param name="delimiterA">Delimiter separating key and value.</param>
        /// <param name="delimiterB">Delimiter separating multiple key/value pairs.</param>
        /// <returns>A text string representing the storage variables.</returns>
        public static string ExportToText(string delimiterA = "=",  string delimiterB = ",")
        {
            string output = string.Empty;

            for (int i = 0; i < Vars.Count; i++)
            {
                if (i > 0)
                {
                    output += delimiterB;
                }
                
                output += $"{Vars[i].key}={Vars[i].value}";
            }
            
            return output;
        }

        /// <summary>
        /// Imports storage variables from a text string, replacing the current storage.
        /// </summary>
        /// <param name="text">A text string representing the storage variables.</param>
        /// <param name="delimiterA">Delimiter separating key and value.</param>
        /// <param name="delimiterB">Delimiter separating multiple key/value pairs.</param>
        public static void ImportFromText(string text, string delimiterA = "=",  string delimiterB = ",")
        {
            Clear();
            
            string[] pairs = text.Split(delimiterB);

            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split(delimiterA);
                
                if (keyValue.Length != 2)
                {
                    continue;
                }
                
                Add(keyValue[0], keyValue[1]);
            }
        }

        #endregion
    }
}