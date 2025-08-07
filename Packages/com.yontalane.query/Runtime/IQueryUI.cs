using System;

namespace Yontalane.Query
{
    public interface IQueryUI
    {
        public void Initialize(string text, string description, string[] responses, int initialSelection, Action<QueryEventData> callback, Action<QueryEventData> selectCallback);
    }
}
