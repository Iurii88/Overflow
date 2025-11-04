using System;

namespace Game.Core.Logging.Modules.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class LogModuleAttribute : Attribute
    {
        public bool DefaultEnabled { get; set; }
        public bool EditorOnly { get; set; }

        public LogModuleAttribute(bool defaultEnabled = false, bool editorOnly = false)
        {
            DefaultEnabled = defaultEnabled;
            EditorOnly = editorOnly;
        }
    }
}