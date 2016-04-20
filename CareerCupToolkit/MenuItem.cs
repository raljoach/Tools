namespace CareerCupToolkit
{
    public class Menu
    {
        public Menu(MenuType menuType, Menu previous)
        {
            this.MenuType = menuType;
            this.Previous = previous;
        }

        public MenuType MenuType { get; internal set; }
        public Menu Previous { get; internal set; }
    }
}