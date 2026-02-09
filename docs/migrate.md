dotnet ef migrations add Template --project Kidzgo.Infrastructure --startup-project Kidzgo.API 

dotnet ef database update --project Kidzgo.Infrastructure --startup-project Kidzgo.API
