-- ShopEase: MySQL database for the cart (Assignment 1).
-- The console test program creates this automatically, so running this script is optional.
-- To run it yourself, open it in MySQL Workbench and press the lightning button,
-- or run:  mysql -u root -p < database/shop.sql

CREATE DATABASE IF NOT EXISTS Shop;
USE Shop;

CREATE TABLE IF NOT EXISTS Products (
    CartItemID INT AUTO_INCREMENT PRIMARY KEY,
    ProductID  INT            NOT NULL,
    Name       VARCHAR(100)   NOT NULL,
    Price      DECIMAL(10, 2) NOT NULL,
    Category   VARCHAR(50)    NOT NULL,
    AddedAt    DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_Products_ProductID (ProductID)
);

-- See what the cart holds:
-- SELECT * FROM Products;
