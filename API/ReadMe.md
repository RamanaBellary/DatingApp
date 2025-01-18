Course Source Code: GitHub.com/TryCatchLearn/DatingApp

To Install EF for .net Migration: dotnet tool install --global dotnet-ef --version 8.0.1

To Add DB Migrations: dotnet ef migrations add InitialCreate -o Data/Migrations

To Update DB after Migrations: dotnet ef database update

To Drop DB: dotnet ef database drop