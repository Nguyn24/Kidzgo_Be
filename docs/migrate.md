dotnet ef migrations add TaS --project Kidzgo.Infrastructure --startup-project Kidzgo.API 

dotnet ef database update --project Kidzgo.Infrastructure --startup-project Kidzgo.API
