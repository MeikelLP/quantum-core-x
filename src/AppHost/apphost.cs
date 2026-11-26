#:package Aspire.Hosting.AppHost@17.0.0
#:package Aspire.Hosting.Dotnet@17.0.0
#:package Aspire.Hosting.Mysql@17.0.0
#:package Aspire.Hosting.Valkey@17.0.0

using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

var dbPw = builder.AddParameter("db-pw", true);
var cachePw = builder.AddParameter("cache-pw", true);

var mysql = builder.AddMySql("db", dbPw, 8306)
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("metin2");
var valkey = builder.AddValkey("cache", 8379, cachePw)
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddDotnetProject("auth", "../Executables/Auth")
    .WithEndpoint(11002, 11002, "tcp", "tcp", isProxied: false, protocol: ProtocolType.Tcp)
    .WithReference(mysql)
    .WaitFor(mysql)
    .WithReference(valkey)
    .WaitFor(valkey)
    .WithEnvironment("Database__Provider", "mysql")
    .WithEnvironment("Database__ConnectionString", mysql.Resource)
    .WithEnvironment("Cache__Host", valkey.Resource.Host)
    .WithEnvironment("Cache__Port", valkey.Resource.Port)
    .WithEnvironment("Cache__Password", valkey.Resource.PasswordParameter!)
    .WithOtlpExporter();

builder.AddDotnetProject("game", "../Executables/Game")
    .WithEndpoint(13001, 13001, "tcp", "tcp", isProxied: false, protocol: ProtocolType.Tcp)
    .WithReference(mysql)
    .WaitFor(mysql)
    .WithReference(valkey)
    .WaitFor(valkey)
    .WithEnvironment("Database__Provider", "mysql")
    .WithEnvironment("Database__ConnectionString", mysql.Resource)
    .WithEnvironment("Cache__Host", valkey.Resource.Host)
    .WithEnvironment("Cache__Port", valkey.Resource.Port)
    .WithEnvironment("Cache__Password", valkey.Resource.PasswordParameter!)
    .WithOtlpExporter();

builder.Build().Run();