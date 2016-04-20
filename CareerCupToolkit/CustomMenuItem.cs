using System;
using System.Collections.Generic;

namespace CareerCupToolkit
{
    public class CustomMenu : Menu
    {
        public List<Option> list;
        private Func<OptionsResult> show;

        public CustomMenu(List<Option> list, Menu prev) : base(MenuType.Custom, prev)
        {
            this.list = list;            
        }

        public CustomMenu(List<Option> list, Func<OptionsResult> show, Menu prev) : this(list,prev)
        {
            this.show = show;
        }

        public OptionsResult Show()
        {
            if(show!=null)
            {
                return show();
            }
            return null;
        }
    }
}