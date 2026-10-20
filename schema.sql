-- Схема базы данных ComputerShopDB (для отчёта)

CREATE DATABASE ComputerShopDB;
GO
USE ComputerShopDB;
GO

CREATE TABLE Users (
    UserID INT IDENTITY PRIMARY KEY,
    Login NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(64) NOT NULL,
    Role NVARCHAR(20) NOT NULL
);

CREATE TABLE Category (
    CategoryID INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Manufacturer (
    ManufacturerID INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Customer (
    CustomerID INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(30) NULL,
    Email NVARCHAR(100) NULL,
    UserID INT NULL FOREIGN KEY REFERENCES Users(UserID)
);

CREATE TABLE Product (
    ProductID INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL,
    CategoryID INT NOT NULL FOREIGN KEY REFERENCES Category(CategoryID),
    ManufacturerID INT NOT NULL FOREIGN KEY REFERENCES Manufacturer(ManufacturerID)
);

CREATE TABLE [Order] (
    OrderID INT IDENTITY PRIMARY KEY,
    CustomerID INT NOT NULL FOREIGN KEY REFERENCES Customer(CustomerID),
    [Date] DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT N'Новый'
);

CREATE TABLE OrderItem (
    OrderItemID INT IDENTITY PRIMARY KEY,
    OrderID INT NOT NULL FOREIGN KEY REFERENCES [Order](OrderID),
    ProductID INT NOT NULL FOREIGN KEY REFERENCES Product(ProductID),
    Quantity INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL
);
GO

INSERT INTO Users (Login, PasswordHash, Role) VALUES
('admin', '<SHA-256(admin123)>', N'Админ'),
('manager', '<SHA-256(manager123)>', N'Менеджер'),
('user', '<SHA-256(user123)>', N'Клиент');

INSERT INTO Category (Name) VALUES
(N'Ноутбуки'), (N'Мониторы'), (N'Клавиатуры'), (N'Мыши');

INSERT INTO Manufacturer (Name) VALUES
(N'ASUS'), (N'Lenovo'), (N'Samsung'), (N'LG'), (N'Logitech'), (N'HyperX'), (N'Razer');

INSERT INTO Product (Name, Price, Stock, CategoryID, ManufacturerID) VALUES
(N'Ноутбук ASUS ROG Strix G15', 120000, 5, 1, 1),
(N'Ноутбук Lenovo IdeaPad 3', 55000, 10, 1, 2),
(N'Монитор Samsung Odyssey G5', 30000, 8, 2, 3),
(N'Монитор LG UltraGear 27', 35000, 6, 2, 4),
(N'Клавиатура Logitech G213', 6500, 20, 3, 5),
(N'Клавиатура HyperX Alloy Core', 8900, 15, 3, 6),
(N'Мышь Logitech G502 Hero', 7500, 25, 4, 5),
(N'Мышь Razer DeathAdder V2', 6200, 18, 4, 7);
