## MESS Local Development
This will outline a tutorial on how to get the MESS application running on your local machine.

#### A Note on Using Docker
For local development we have created a Dockerfile that will setup a local instance of the SQL Server database to run tests on. Of course this can be replaced by any other instance of a SQL Server database. NOTE: You may have to modify the connection string for the database if you deviate from this guide.

### 1. Ensure that you have Docker **installed** and **running** on your machine.

Follow this guide for your desired Operating System if you currently do not have Docker installed: https://docs.docker.com/desktop/setup/install/windows-install/

### 2. Creating a SQL Server Docker Container
Open a bash or Windows terminal and enter the following commands:

These commands come from the [Docker Hub for SQL Server](https://hub.docker.com/r/microsoft/mssql-server)

This command will download and start a Docker instance of a **SQL Server version 2022** on the **Linux Ubuntu 22.04** OS

SQL Server requires complex passwords so you'll need to replace `ENTER_YOUR_DB_PASSWORD_HERE` with a secure and complicated password.

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=ENTER_YOUR_DB_PASSWORD_HERE" -e "MSSQL_PID=Evaluation" -p 1433:1433  --name MESS_Data -d mcr.microsoft.com/mssql/server:2022-preview-ubuntu-22.04
```
#### Apple Silicon Users (M1, etc.):
Microsoft SQL Server does not have a native ARM build, so you must run the Intel version under emulation. This is done by adding **--platform linux/amd64** to your docker run command. The rest of the setup is identical.

##### macOS Apple Silicon (M1, etc.) and ARM-based systems:
```bash
docker run --platform linux/amd64 -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=ENTER_YOUR_DB_PASSWORD_HERE" -e "MSSQL_PID=developer" -p 1433:1433 --name MESS_Data -d mcr.microsoft.com/mssql/server:2022-latest
```
NOTE: SQL Server password requirements can be found [here](https://learn.microsoft.com/en-us/sql/relational-databases/security/password-policy?view=sql-server-ver17).

### 3. Connecting to the Database
To connect to the database locally ensure that you have setup the connection string. For instructions on how to do so please refer to the section on setting up environment variables and secrets.

At this stage if the Docker container has no errors and is running, you should be able to connect to the SQL Server instance with a 3rd party tool such as:

* SQL Server Management Studio
* Azure Data Manager
* IDE Database Extensions

If you are unable to connect to the database at this stage, ensure that the Docker instance has no errors.

### 4. Cloning the Repository
At this point you can make a new project in your IDE of choice and clone MESS repository during its creation. This process will vary depending upon your development environment.

### 5. Connection String
In new development environments, navigate to the MESS.Data directory and use the following command to set a user secret (remember to use your database password from earlier):

```shell
dotnet user-secrets set "ConnectionStrings:MESSConnection" "Server=localhost; Database=MESS_Data; User Id=sa; Password=ENTER_YOUR_DB_PASSWORD_HERE; TrustServerCertificate=True;"
```

The connection string within the secrets file should look like:

```json
  "ConnectionStrings:MESSConnection": "Server=localhost; Database=MESS_Data; User Id=sa; Password=ENTER_YOUR_DB_PASSWORD_HERE; TrustServerCertificate=True;"
```

NOTE: If you recieve an error when attempting to apply the Entity Framework Core migrations you may have to manually create the _MESS_Data_ database within the SQL Server instance. You should be able to use an IDE database extension, SQL Server Management Studio, or some other database tool to alter the instance.


### 6. Applying Database Migrations
Once the connection string is set, you will need to update the database with 2 sets of migrations. The first being **ApplicationContext** which contains the database logic for the domain of MESS. And the second being **UserContext** which contains the models for Microsoft's Identity, and is required for authentication and authorization.

Note:  On Windows, you may need to run this first:
```bash
dotnet tool install --global dotnet-ef
```

To apply these migrations run the following commands in the _MESS.Data_ project in the CLI.

```bash
dotnet ef database update --context ApplicationContext --startup-project ..\MESS.Blazor\MESS.Blazor.csproj
dotnet ef database update --context UserContext --startup-project ..\MESS.Blazor\MESS.Blazor.csproj
```
##### macOS Apple Silicon (M1, etc.) and ARM-based systems:
```bash
dotnet ef database update --context ApplicationContext --startup-project ../MESS.Blazor/MESS.Blazor.csproj
dotnet ef database update --context UserContext --startup-project ../MESS.Blazor/MESS.Blazor.csproj
```
Note: When applying database migrations, if you encounter an error saying "Invalid Object Name" you may have to run the other update command first and then try agian. 
### 7. Seeders
At this stage MESS is ready for development, but before you start the project it is important to note that there are several seeders that will populate the SQL Server database with test data. This includes a default TechnicianUser, along with an assortment of test Products, WorkInstructions, Parts, etc.
The seeders only run when the tables have no entries in them, but they are independent of each other. So if the Users table has 5 users, but you decide to wipe out the Work Instructions table, the seeder will populate the Work Instructions table but not the Users table. To run MESS, simply run the Blazor project from within your IDE.


### Wiping the Database
Upon deletion of the Docker container ALL data will be lost. This can be avoided with the use of **Volumes** but this guide will not show how. For a quick start of all the steps above here is the Docker run command which will setup the SQL Server database with all the configurations preset EXCEPT for the password which you will have to set:

```txt
docker run --hostname=3354685396dc --user=mssql --env=ACCEPT_EULA=Y --env=MSSQL_SA_PASSWORD=ENTER_YOUR_DB_PASSWORD_HERE --env=MSSQL_PID=Evaluation --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --env=MSSQL_RPC_PORT=135 --env=CONFIG_EDGE_BUILD= --network=bridge -p 1433:1433 --restart=no --label='com.microsoft.product=Microsoft SQL Server' --label='com.microsoft.version=16.0.4085.2' --label='org.opencontainers.image.ref.name=ubuntu' --label='org.opencontainers.image.version=22.04' --label='vendor=Microsoft' --runtime=runc -d mcr.microsoft.com/mssql/server:2022-preview-ubuntu-22.04
```

After running this command you will also need to follow step 5 again to reapply the migrations.

#### IDE
For the MESS application the IDE should not matter. During initial development the team used a variety of Open Source Compliant IDE's and Editors including:
- Visual Studio Community
- Visual Studio Code
- JetBrain's Rider