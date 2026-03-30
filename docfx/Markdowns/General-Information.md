# General Information regarding MESS (Manufacturing Execution Software System)

## Overview

### Technologies Used
* SDK: **.NET 9.0**
* Web Framework: **Blazor**
* Database (although changeable with a little bit of effort): **Microsoft's SQL-Server**
* Testing Framework: xUnit
* UI Library: FluentUI

### Project Structure

- **MESS.Blazor:** Frontend Project that stores the User Interface
- **MESS.Data:** Project dedicated to storing database models, seeders, database logic, migrations, and more. Reasoning behind having a data project is for the anticipation of a future WPF or similar desktop application, that may utilize the same data layer logic.
- **MESS.Services:** Stores all services for the application. Essentially the core logic that does not directly change the UI of MESS is located here. Similar to MESS.Data, Services is its own project in anticipation of a migration to WPF in the future.
- **MESS.Tests:** Stores all tests for the entire MESS Solution.

### Prerequisites
* .NET 9.0
* A running instance of SQL Server
* A basic understanding of SQL (Structured Query Language) and SQL Server

### System Requirements
* Dual-Core CPU
* 2GB RAM
* High Speed bandwidth