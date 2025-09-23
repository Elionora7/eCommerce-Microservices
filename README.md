# eCommerce-Microservices

eCommerce Microservices – Project Documentation

Overview

Polyglot microservices eCommerce application demonstrating independent service development, deployment, and database ownership.



The primary frontend is built with React and includes an optional Angular UI for comparison.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

✨ Key Features



Product catalogue with filtering and search



Shopping basket and order processing



User registration and authentication



API Gateway to unify access to all services



Each microservice owns its own database (polyglot persistence)

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_



🏗 Architecture

&nbsp;                          ┌───────────────────┐

&nbsp;                          │  React Frontend   │

&nbsp;                          │  (Vite + TS)      │

&nbsp;                          │  + optional       │

&nbsp;                          │  Angular Frontend │

&nbsp;                          └─────────┬─────────┘

&nbsp;                                    │

&nbsp;                          ┌─────────┴─────────┐

&nbsp;                          │    API Gateway    │

&nbsp;                          │      (Ocelot)     │

&nbsp;       ┌──────────────────┴─────┬─────────────┴─────────────────┐

&nbsp;       ▼                        ▼                               ▼

┌───────────────┐      ┌────────────────┐               ┌────────────────┐

│ ProductService │      │ OrderService  │               │ UserService    │

│ + SQL Server   │      │ + PostgreSQL  │               │ + MongoDB      │

└───────────────┘      └────────────────┘               └────────────────┘



\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

Databases



ProductService: SQL Server



OrderService: PostgreSQL



UserService: MongoDB

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

🧪 Running Locally



Prerequisites



Docker \& Docker Compose



.NET 6+ SDK (only if running outside Docker)



git clone https://github.com/Elionora7/eCommerce-Microservices.git

cd eCommerce-Microservices

docker-compose up --build



React Frontend: http://localhost:5173



API Gateway: http://localhost:4500/gateway/



Optional Angular frontend under /angular-client

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

🔎 Advanced Features \& Architecture Details



Clean Architecture + CQRS in each microservice



AutoMapper for DTO ↔ Entity mapping



FluentValidation for request validation



Redis caching layer for product catalogue



RabbitMQ message broker for asynchronous order events



Ocelot API Gateway: routing, request aggregation, and rate limiting



JWT Authentication for secure endpoints



Microservice Highlights

Service	Key Tech \& Features

ProductService	ASP.NET Core • EF Core + SQL Server • Redis caching • FluentValidation

OrderService	ASP.NET Core • EF Core + PostgreSQL • RabbitMQ events • Unit tests with Moq \& xUnit

UserService	ASP.NET Core • MongoDB • JWT Authentication

🧪 Testing



Unit Tests: Implemented in the OrderService using xUnit and Moq (mock services).



Covers business logic for order creation, validation, and repository interactions.



Run tests:



cd OrdersService.Tests

dotnet test

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

🚀 Deployment Options



Demo Video: Screen-recorded end-to-end walkthrough of the application, showing frontend, API calls, and database updates.



Azure Deployment Documentation: Step-by-step guide (with screenshots) for deploying all services and databases to Azure AKS and ACR: 
📄 Full Azure Deployment Guide:
See [`docs/Azure Deployment Documentation.docx`](https://github.com/Elionora7/eCommerce-Microservices/blob/main/docs/Azure%20Deployment%20Documentation.docx)


\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

🛠 Tech Stack



Layer	            Technology

Frontend            React, TypeScript, Vite, React Query (+ optional Angular)

Gateway 	    Ocelot

ProductService	    ASP.NET Core, EF Core, SQL Server

OrderService	    ASP.NET Core, EF Core, PostgreSQL

UserService	    ASP.NET Core, MongoDB

Messaging	    RabbitMQ

Caching		    Redis

Containerization    Docker, Docker Compose

Testing		    xUnit, Moq

CI/CD		    GitHub Actions 

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

📚 Documentation \& Assets



Architecture diagram

Database schemas or sample scripts for each DB

Azure Deployment guide (with screenshots)

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

🧩 Future Improvements



More integration and end-to-end tests

Centralized logging \& monitoring (e.g., Serilog + ELK)

Horizontal scaling on Kubernetes / Azure AKS

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

👤 Author

Elionora El Dahan

•	GitHub: Elionora7

•	LinkedIn: Elionora El Dahan | LinkedIn

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_



