
cd ../../Host/SlaveManager
rmdir /S /Q Migrations
dotnet build .

dotnet ef database drop &
dotnet ef migrations add Deployment &
dotnet ef database update



