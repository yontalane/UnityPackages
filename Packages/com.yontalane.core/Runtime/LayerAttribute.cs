using System;
using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// Display a layer dropdown menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class LayerAttribute : PropertyAttribute
    {
        public LayerAttribute()
        {

        }
    }
}
