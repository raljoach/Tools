using System;

namespace CareerCupToolkit
{
    public class Option
    {
        public static string ENTER = "Enter";

        public Option(string description, string shortcut, Action action)
        {
            this.Description = description;
            this.Shortcut = shortcut;
            this.Execute = action;
        }

        public int Id { get; internal set; }
        public string Description { get; internal set; }
        public string Shortcut { get; internal set; }
        public virtual Action Execute { get; internal set; }
        public string DisplayShortcut { get { return GetDisplayShortcut(); } }

        public override string ToString()
        {
            return string.Format(
                "{0} [{1}]", this.Description,this.DisplayShortcut);
        }

        private string GetDisplayShortcut()
        {
            if(string.IsNullOrWhiteSpace(Shortcut))
            {
                return ENTER;
            }
            return Shortcut;
        }
    }
}