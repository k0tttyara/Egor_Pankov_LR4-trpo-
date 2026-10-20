using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace ComputerShop
{
    public static class Db
    {
        public static readonly string ConnStr =
            @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=ComputerShopDB;Integrated Security=True;";


        public static string Sha256(string s)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sb = new StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static void EnsureDatabase()
        {
            using (var conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True;"))
            {
                conn.Open();
                new SqlCommand("IF DB_ID(N'ComputerShopDB') IS NULL CREATE DATABASE ComputerShopDB", conn).ExecuteNonQuery();
            }

            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                string sql = @"
IF OBJECT_ID(N'Users') IS NULL
    CREATE TABLE Users (
        UserID INT IDENTITY PRIMARY KEY,
        Login NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(64) NOT NULL,
        Role NVARCHAR(20) NOT NULL);

IF OBJECT_ID(N'Category') IS NULL
    CREATE TABLE Category (
        CategoryID INT IDENTITY PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL);

IF OBJECT_ID(N'Manufacturer') IS NULL
    CREATE TABLE Manufacturer (
        ManufacturerID INT IDENTITY PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL);

IF OBJECT_ID(N'Customer') IS NULL
    CREATE TABLE Customer (
        CustomerID INT IDENTITY PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        Phone NVARCHAR(30) NULL,
        Email NVARCHAR(100) NULL,
        UserID INT NULL FOREIGN KEY REFERENCES Users(UserID));

IF OBJECT_ID(N'Product') IS NULL
    CREATE TABLE Product (
        ProductID INT IDENTITY PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Price DECIMAL(10,2) NOT NULL,
        Stock INT NOT NULL,
        CategoryID INT NOT NULL FOREIGN KEY REFERENCES Category(CategoryID),
        ManufacturerID INT NOT NULL FOREIGN KEY REFERENCES Manufacturer(ManufacturerID));

IF OBJECT_ID(N'[Order]') IS NULL
    CREATE TABLE [Order] (
        OrderID INT IDENTITY PRIMARY KEY,
        CustomerID INT NOT NULL FOREIGN KEY REFERENCES Customer(CustomerID),
        [Date] DATETIME NOT NULL DEFAULT GETDATE(),
        Status NVARCHAR(20) NOT NULL DEFAULT N'Новый');

IF OBJECT_ID(N'OrderItem') IS NULL
    CREATE TABLE OrderItem (
        OrderItemID INT IDENTITY PRIMARY KEY,
        OrderID INT NOT NULL FOREIGN KEY REFERENCES [Order](OrderID),
        ProductID INT NOT NULL FOREIGN KEY REFERENCES Product(ProductID),
        Quantity INT NOT NULL,
        Price DECIMAL(10,2) NOT NULL);";
                new SqlCommand(sql, conn).ExecuteNonQuery();

                SeedIfEmpty(conn);
            }
        }

        private static void SeedIfEmpty(SqlConnection conn)
        {
            int cnt = Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM Users", conn).ExecuteScalar());
            if (cnt > 0) return;

            var users = new[] {
                new[] { "admin", "Админ" }, new[] { "manager", "Менеджер" }, new[] { "user", "Клиент" }
            };
            foreach (var u in users)
            {
                var cmd = new SqlCommand("INSERT INTO Users (Login, PasswordHash, Role) VALUES (@l, @h, @r)", conn);
                cmd.Parameters.AddWithValue("@l", u[0]);
                cmd.Parameters.AddWithValue("@h", Sha256(u[0] + "123"));
                cmd.Parameters.AddWithValue("@r", u[1]);
                cmd.ExecuteNonQuery();
            }

            var cats = new[] { "Ноутбуки", "Мониторы", "Клавиатуры", "Мыши" };
            var mans = new[] { "ASUS", "Lenovo", "Samsung", "LG", "Logitech", "HyperX", "Razer" };
            foreach (var c in cats) new SqlCommand("INSERT INTO Category (Name) VALUES (N'" + c + "')", conn).ExecuteNonQuery();
            foreach (var m in mans) new SqlCommand("INSERT INTO Manufacturer (Name) VALUES (N'" + m + "')", conn).ExecuteNonQuery();

            var products = new[,] {
                { "Ноутбук ASUS ROG Strix G15",   "120000", "5",  "Ноутбуки",   "ASUS" },
                { "Ноутбук Lenovo IdeaPad 3",     "55000",  "10", "Ноутбуки",   "Lenovo" },
                { "Монитор Samsung Odyssey G5",   "30000",  "8",  "Мониторы",   "Samsung" },
                { "Монитор LG UltraGear 27",      "35000",  "6",  "Мониторы",   "LG" },
                { "Клавиатура Logitech G213",     "6500",   "20", "Клавиатуры", "Logitech" },
                { "Клавиатура HyperX Alloy Core", "8900",   "15", "Клавиатуры", "HyperX" },
                { "Мышь Logitech G502 Hero",      "7500",   "25", "Мыши",       "Logitech" },
                { "Мышь Razer DeathAdder V2",     "6200",   "18", "Мыши",       "Razer" }
            };
            for (int i = 0; i < products.GetLength(0); i++)
            {
                var cmd = new SqlCommand(@"
INSERT INTO Product (Name, Price, Stock, CategoryID, ManufacturerID)
VALUES (@n, @p, @s,
    (SELECT CategoryID FROM Category WHERE Name = @c),
    (SELECT ManufacturerID FROM Manufacturer WHERE Name = @m))", conn);
                cmd.Parameters.AddWithValue("@n", products[i, 0]);
                cmd.Parameters.AddWithValue("@p", decimal.Parse(products[i, 1]));
                cmd.Parameters.AddWithValue("@s", int.Parse(products[i, 2]));
                cmd.Parameters.AddWithValue("@c", products[i, 3]);
                cmd.Parameters.AddWithValue("@m", products[i, 4]);
                cmd.ExecuteNonQuery();
            }
        }


        public static User GetUser(string login, string passwordHash)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT UserID, Login, Role FROM Users WHERE Login = @l AND PasswordHash = @h", conn);
                cmd.Parameters.AddWithValue("@l", login);
                cmd.Parameters.AddWithValue("@h", passwordHash);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return new User
                        {
                            UserID = reader.GetInt32(0),
                            Login = reader.GetString(1),
                            Role = reader.GetString(2)
                        };
                }
            }
            return null;
        }

        public static bool RegisterUser(string login, string passwordHash, string fio, string email, string phone)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var check = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Login = @l", conn);
                check.Parameters.AddWithValue("@l", login);
                if (Convert.ToInt32(check.ExecuteScalar()) > 0) return false;

                var cmdUser = new SqlCommand(
                    "INSERT INTO Users (Login, PasswordHash, Role) VALUES (@l, @h, N'Клиент'); SELECT SCOPE_IDENTITY();", conn);
                cmdUser.Parameters.AddWithValue("@l", login);
                cmdUser.Parameters.AddWithValue("@h", passwordHash);
                int userId = Convert.ToInt32(cmdUser.ExecuteScalar());

                var cmdCust = new SqlCommand(
                    "INSERT INTO Customer (FullName, Phone, Email, UserID) VALUES (@n, @p, @e, @u)", conn);
                cmdCust.Parameters.AddWithValue("@n", fio);
                cmdCust.Parameters.AddWithValue("@p", phone);
                cmdCust.Parameters.AddWithValue("@e", email);
                cmdCust.Parameters.AddWithValue("@u", userId);
                cmdCust.ExecuteNonQuery();
            }
            return true;
        }

        public static int GetOrCreateCustomer(User user)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT TOP 1 CustomerID FROM Customer WHERE UserID = @u", conn);
                cmd.Parameters.AddWithValue("@u", user.UserID);
                var obj = cmd.ExecuteScalar();
                if (obj != null) return Convert.ToInt32(obj);

                var ins = new SqlCommand(
                    "INSERT INTO Customer (FullName, Phone, Email, UserID) VALUES (@n, N'', @e, @u); SELECT SCOPE_IDENTITY();", conn);
                ins.Parameters.AddWithValue("@n", user.Login);
                ins.Parameters.AddWithValue("@e", user.Login + "@shop.local");
                ins.Parameters.AddWithValue("@u", user.UserID);
                return Convert.ToInt32(ins.ExecuteScalar());
            }
        }


        public static List<Category> GetCategories()
        {
            var list = new List<Category>();
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                using (var reader = new SqlCommand("SELECT CategoryID, Name FROM Category ORDER BY Name", conn).ExecuteReader())
                    while (reader.Read())
                        list.Add(new Category { CategoryID = reader.GetInt32(0), Name = reader.GetString(1) });
            }
            return list;
        }

        public static List<Product> GetProducts(int? categoryId, string search)
        {
            var list = new List<Product>();
            string sql = @"
SELECT p.ProductID, p.Name, p.Price, p.Stock, p.CategoryID, p.ManufacturerID,
       ISNULL(c.Name, '') AS CategoryName, ISNULL(m.Name, '') AS ManufacturerName
FROM Product p
LEFT JOIN Category c ON p.CategoryID = c.CategoryID
LEFT JOIN Manufacturer m ON p.ManufacturerID = m.ManufacturerID
WHERE 1 = 1";
            if (categoryId.HasValue) sql += " AND p.CategoryID = @cat";
            if (!string.IsNullOrEmpty(search)) sql += " AND p.Name LIKE @s";
            sql += " ORDER BY p.Name";

            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var cmd = new SqlCommand(sql, conn);
                if (categoryId.HasValue) cmd.Parameters.AddWithValue("@cat", categoryId.Value);
                if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@s", "%" + search + "%");
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new Product
                        {
                            ProductID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Price = reader.GetDecimal(2),
                            Stock = reader.GetInt32(3),
                            CategoryID = reader.GetInt32(4),
                            ManufacturerID = reader.GetInt32(5),
                            CategoryName = reader.GetString(6),
                            ManufacturerName = reader.GetString(7)
                        });
            }
            return list;
        }


        public static void PlaceOrder(int customerId, List<CartItem> cart)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    foreach (var item in cart)
                    {
                        var cmd = new SqlCommand(
                            "UPDATE Product SET Stock = Stock - @q WHERE ProductID = @id AND Stock >= @q", conn, tran);
                        cmd.Parameters.AddWithValue("@q", item.Quantity);
                        cmd.Parameters.AddWithValue("@id", item.Product.ProductID);
                        if (cmd.ExecuteNonQuery() == 0)
                            throw new Exception("Недостаточно товара на складе: " + item.Product.Name);
                    }

                    var cmdOrder = new SqlCommand(
                        "INSERT INTO [Order] (CustomerID, [Date], Status) VALUES (@c, GETDATE(), N'Новый'); SELECT SCOPE_IDENTITY();",
                        conn, tran);
                    cmdOrder.Parameters.AddWithValue("@c", customerId);
                    int orderId = Convert.ToInt32(cmdOrder.ExecuteScalar());

                    foreach (var item in cart)
                    {
                        var cmd = new SqlCommand(
                            "INSERT INTO OrderItem (OrderID, ProductID, Quantity, Price) VALUES (@o, @p, @q, @pr)", conn, tran);
                        cmd.Parameters.AddWithValue("@o", orderId);
                        cmd.Parameters.AddWithValue("@p", item.Product.ProductID);
                        cmd.Parameters.AddWithValue("@q", item.Quantity);
                        cmd.Parameters.AddWithValue("@pr", item.Price);
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        public static List<Order> GetOrders(int? forCustomer)
        {
            var list = new List<Order>();
            string sql = @"
SELECT o.OrderID, o.[Date], o.Status, c.FullName,
       ISNULL(SUM(oi.Quantity * oi.Price), 0) AS Total
FROM [Order] o
JOIN Customer c ON o.CustomerID = c.CustomerID
LEFT JOIN OrderItem oi ON oi.OrderID = o.OrderID";
            if (forCustomer.HasValue) sql += " WHERE o.CustomerID = @cid";
            sql += " GROUP BY o.OrderID, o.[Date], o.Status, c.FullName ORDER BY o.OrderID DESC";

            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var cmd = new SqlCommand(sql, conn);
                if (forCustomer.HasValue) cmd.Parameters.AddWithValue("@cid", forCustomer.Value);
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new Order
                        {
                            OrderID = reader.GetInt32(0),
                            Date = reader.GetDateTime(1),
                            Status = reader.GetString(2),
                            CustomerName = reader.GetString(3),
                            Total = reader.GetDecimal(4)
                        });
            }
            return list;
        }

        public static List<OrderItem> GetOrderItems(int orderId)
        {
            var list = new List<OrderItem>();
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
SELECT oi.OrderItemID, oi.OrderID, oi.ProductID, p.Name, oi.Quantity, oi.Price
FROM OrderItem oi JOIN Product p ON oi.ProductID = p.ProductID
WHERE oi.OrderID = @o ORDER BY oi.OrderItemID", conn);
                cmd.Parameters.AddWithValue("@o", orderId);
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new OrderItem
                        {
                            OrderItemID = reader.GetInt32(0),
                            OrderID = reader.GetInt32(1),
                            ProductID = reader.GetInt32(2),
                            ProductName = reader.GetString(3),
                            Quantity = reader.GetInt32(4),
                            Price = reader.GetDecimal(5)
                        });
            }
            return list;
        }

        private static void ReturnStock(int orderId, SqlConnection conn, SqlTransaction tran)
        {
            var cmd = new SqlCommand(@"
UPDATE Product SET Stock = Stock + oi.Quantity
FROM Product p JOIN OrderItem oi ON oi.ProductID = p.ProductID
WHERE oi.OrderID = @o", conn, tran);
            cmd.Parameters.AddWithValue("@o", orderId);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateOrderStatus(int orderId, string newStatus)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    var get = new SqlCommand("SELECT Status FROM [Order] WHERE OrderID = @o", conn, tran);
                    get.Parameters.AddWithValue("@o", orderId);
                    string oldStatus = Convert.ToString(get.ExecuteScalar());

                    if (oldStatus == "Новый" && newStatus == "Отменён")
                        ReturnStock(orderId, conn, tran);

                    var upd = new SqlCommand("UPDATE [Order] SET Status = @s WHERE OrderID = @o", conn, tran);
                    upd.Parameters.AddWithValue("@s", newStatus);
                    upd.Parameters.AddWithValue("@o", orderId);
                    upd.ExecuteNonQuery();

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        public static void DeleteOrder(int orderId)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    ReturnStock(orderId, conn, tran);

                    var cmd1 = new SqlCommand("DELETE FROM OrderItem WHERE OrderID = @o", conn, tran);
                    cmd1.Parameters.AddWithValue("@o", orderId);
                    cmd1.ExecuteNonQuery();

                    var cmd2 = new SqlCommand("DELETE FROM [Order] WHERE OrderID = @o", conn, tran);
                    cmd2.Parameters.AddWithValue("@o", orderId);
                    cmd2.ExecuteNonQuery();

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }
    }
}
