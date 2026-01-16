# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj first for caching
COPY *.sln ./
COPY MESS.Blazor/*.csproj MESS.Blazor/
COPY MESS.Data/*.csproj MESS.Data/
COPY MESS.Services/*.csproj MESS.Services/
COPY MESS.Tests/*.csproj MESS.Tests/

RUN dotnet restore 

#Copy rest of the repo
COPY . .

RUN dotnet publish ./MESS.Blazor -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MESS.Blazor.dll"]