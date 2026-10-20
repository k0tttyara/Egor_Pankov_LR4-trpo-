using System;
using System.Collections.Generic;

namespace ComputerShop
{
    public class User
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
    }

    public class Manufacturer
    {
        public int ManufacturerID { get; set; }
        public string Name { get; set; }
    }

    public class Customer
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryID { get; set; }
        public int ManufacturerID { get; set; }
        public string CategoryName { get; set; }
        public string ManufacturerName { get; set; }
    }

    public class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Sum { get { return Price * Quantity; } }
    }

    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get { return Product.Price; } }
        public decimal Sum { get { return Product.Price * Quantity; } }
    }
}
