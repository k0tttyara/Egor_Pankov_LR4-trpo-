using System.Collections.Generic;

namespace ComputerShop
{
    public static class Session
    {
        public static User CurrentUser;
        public static List<CartItem> Cart = new List<CartItem>();

        public static bool IsClient  { get { return CurrentUser != null && CurrentUser.Role == "Клиент"; } }
        public static bool IsManager { get { return CurrentUser != null && CurrentUser.Role == "Менеджер"; } }
        public static bool IsAdmin   { get { return CurrentUser != null && CurrentUser.Role == "Админ"; } }
    }
}
