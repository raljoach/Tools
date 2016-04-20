using System;

namespace CareerCupToolkit
{
    public class ValueOption<T> : Option
    {
        private Func<T> function;

        public ValueOption(string description, string shortcut, Func<T> function) : 
            base(description,shortcut,()=> { function(); })
        {
            this.function = function;
        }

        public T ExecuteFunction() { return function(); }
    }
}