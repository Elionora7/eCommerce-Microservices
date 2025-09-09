-- Create the database
CREATE DATABASE IF NOT EXISTS ecommerceproductsdatabase;
USE ecommerceproductsdatabase;

-- Create the products table with imgUrl column
CREATE TABLE IF NOT EXISTS Products (
  ProductID char(36) NOT NULL,
  ProductName varchar(50) NOT NULL,
  Category varchar(50) DEFAULT NULL,
  UnitPrice decimal(10,2) DEFAULT NULL,
  QuantityInStock int DEFAULT NULL,
  imgUrl varchar(255) DEFAULT NULL, -- Added image URL column
  PRIMARY KEY (ProductID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Insert 12 sample rows into the products table with specified categories and image URLs
INSERT INTO Products (ProductID, ProductName, Category, UnitPrice, QuantityInStock, imgUrl) VALUES
  ('1a9df78b-3f46-4c3d-9f2a-1b9f69292a77', 'Apple iPhone 16 Pro Max', 'Electronics', 1299.99, 50, 'https://images.unsplash.com/photo-1592899677977-9c10ca588bbd?w=400'),
  ('2c8e8e7c-97a3-4b11-9a1b-4dbe681cfe17', 'Samsung Foldable Smart Phone 2', 'Electronics', 1499.99, 100, 'https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=400'),
  ('3f3e8b3a-4a50-4cd0-8d8e-1e178ae2cfc1', 'Ergonomic Office Chair', 'Furniture', 249.99, 25, 'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=400'),
  ('4c9b6f71-6c5d-485f-8db2-58011a236b63', 'Coffee Table with Storage', 'Furniture', 179.99, 30, 'https://images.unsplash.com/photo-1533090368676-1fd25485db88?w=400'),
  ('5d7e36bf-65c3-4a71-bf97-740d561d8b65', 'Samsung QLED 95 inch', 'Electronics', 1999.99, 20, 'https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?w=400'),
  ('6a14f510-72c1-42c8-9a5a-8ef8f3f45a0d', 'Running Shoes', 'Sportswear', 49.99, 75, 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400'),
  ('7b39ef14-932b-4c84-9187-55b748d2b28f', 'Anti-Theft Laptop Backpack', 'Accessories', 59.99, 60, 'https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400'),
  ('8c5f6e73-68fc-49d9-99b4-aecc3706a4f4', 'LG OLED 85 inch', 'Electronics', 1499.99, 15, 'https://images.unsplash.com/photo-1593784991095-a205069470b6?w=400'),
  ('9e7e7085-6f4e-4921-8f15-c59f084080f9', 'Modern Dining Table', 'Furniture', 699.99, 10, 'https://images.unsplash.com/photo-1551298370-9d3d53740c72?w=400'),
  ('10d7b110-ecdb-4921-85a4-58a5d1b32bf4', 'PlayStation 5', 'Electronics', 499.99, 40, 'https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=400'),
  ('11f2e86a-9d5d-42f9-b3c2-3e4d652e3df8', 'Executive Office Desk', 'Furniture', 299.99, 18, 'https://images.unsplash.com/photo-1524758631624-e2822e304c36?ixlib=rb-4.0.3&auto=format&fit=crop&w=400&q=80'),
  ('12b369b7-9101-41b1-a653-6c6c9a4fe1e4', 'Breville Smart Blender', 'HomeAppliances', 129.99, 50, 'https://images.unsplash.com/photo-1570222094114-d054a817e56b?w=400');