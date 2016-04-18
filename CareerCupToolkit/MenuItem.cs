namespace CareerCupToolkit
{
    internal class MenuItem
    {
        public MenuItem(MenuType name, MenuType parent, MenuItem previous)
        {
            this.Name = name;
            this.Parent = parent;
            this.Previous = previous;
        }

        public MenuType Name { get; internal set; }
        public MenuType Parent { get; internal set; }
        public MenuItem Previous { get; internal set; }
    }
}