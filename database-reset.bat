cd Host
cd SlaveManager
rmdir /s /q Migrations
dotnet ef
dotnet build .
dotnet ef database drop
dotnet ef migrations add Initial
dotnet ef database update
cd ..
cd ..