using System;
using System.Collections.Generic;

namespace Game.Core.Content
{
    [Serializable]
    public class ContentIndex
    {
        public List<ContentIndexEntry> entries = new();
    }

    [Serializable]
    public class ContentIndexEntry
    {
        public string schema;
        public string id;
        public string addressablePath;
    }
}
