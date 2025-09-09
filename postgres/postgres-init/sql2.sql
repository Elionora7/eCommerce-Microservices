CREATE TABLE IF NOT EXISTS public."Users"
(
    "UserID" uuid NOT NULL,
    "Name" character varying(50) NOT NULL,
    "Email" character varying(50) NOT NULL UNIQUE,
    "Password" character varying(50) NOT NULL,
    "Gender" character varying(15),
    "RefreshToken" TEXT,
    "RefreshTokenExpiryTime" TIMESTAMP,
    CONSTRAINT "Users_pkey" PRIMARY KEY ("UserID")
);

-- Sample data for insertion
 INSERT INTO public."Users" ("UserID", "Email", "Name", "Gender", "Password")
 VALUES 
 ('c32f8b42-60e6-4c02-90a7-9143ab37189f', 'test1@example.com', 'John Doe', 'Male', 'password1'),
 ('8ff22c7d-18c7-4ef0-a0ac-988ecb2ac7f5', 'test2@example.com', 'Jane Smith', 'Female', 'password2');
