using System;
using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// Display a tag dropdown menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class TagAttribute : PropertyAttribute
    {
        public TagAttribute()
        {

        }
    }
}
