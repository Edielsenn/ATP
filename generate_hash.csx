// Script para gerar hash BCrypt
#r "nuget: BCrypt.Net-Next, 4.0.3"
using BCrypt.Net;

var senha = "Admin@123";
var hash = BCrypt.Net.BCrypt.HashPassword(senha);
Console.WriteLine($"Hash para '{senha}':");
Console.WriteLine(hash);
