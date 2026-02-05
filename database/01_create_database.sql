-- Script de criação do banco de dados PNHDigitalDB
-- Execute este script primeiro

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PNHDigitalDB')
BEGIN
    CREATE DATABASE PNHDigitalDB;
    PRINT 'Banco de dados PNHDigitalDB criado com sucesso!';
END
ELSE
BEGIN
    PRINT 'Banco de dados PNHDigitalDB já existe.';
END
GO

USE PNHDigitalDB;
GO
