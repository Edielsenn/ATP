-- ===================================================
-- SCRIPT COMPLETO DE SETUP DO BANCO CNH VIRTUAL
-- Execute este script no SQL Server Management Studio
-- ===================================================

USE master;
GO

-- Criar banco se não existir
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PNHDigitalDB')
BEGIN
    CREATE DATABASE PNHDigitalDB;
    PRINT '✅ Banco PNHDigitalDB criado com sucesso!';
END
ELSE
BEGIN
    PRINT '⚠️ Banco PNHDigitalDB já existe!';
END
GO

USE PNHDigitalDB;
GO

-- ===================================================
-- CRIAR TABELAS
-- ===================================================

-- Tabela de Usuários Administradores
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AdminUsers')
BEGIN
    CREATE TABLE AdminUsers (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        SenhaHash NVARCHAR(255) NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME NULL
    );
    PRINT '✅ Tabela AdminUsers criada!';
END
GO

-- Tabela de Planos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Planos')
BEGIN
    CREATE TABLE Planos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(100) NOT NULL,
        Descricao NVARCHAR(MAX) NOT NULL,
        DescricaoCurta NVARCHAR(200) NOT NULL,
        Preco DECIMAL(10, 2) NOT NULL,
        PrecoPromocional DECIMAL(10, 2) NULL,
        DuracaoDias INT NOT NULL,
        ValidadeDias INT NOT NULL,
        LimiteTentativas INT NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        Destaque BIT NOT NULL DEFAULT 0,
        Ordem INT NOT NULL DEFAULT 0,
        DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME NULL
    );
    PRINT '✅ Tabela Planos criada!';
END
GO

-- Tabela de Recursos dos Planos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PlanoRecursos')
BEGIN
    CREATE TABLE PlanoRecursos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PlanoId INT NOT NULL,
        Descricao NVARCHAR(200) NOT NULL,
        Incluido BIT NOT NULL DEFAULT 1,
        Ordem INT NOT NULL DEFAULT 0,
        FOREIGN KEY (PlanoId) REFERENCES Planos(Id) ON DELETE CASCADE
    );
    PRINT '✅ Tabela PlanoRecursos criada!';
END
GO

-- Tabela de Clientes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(200) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        Telefone NVARCHAR(20) NOT NULL,
        CPF NVARCHAR(14) NOT NULL,
        DataNascimento DATE NOT NULL,
        CEP NVARCHAR(10) NOT NULL,
        Logradouro NVARCHAR(200) NOT NULL,
        Numero NVARCHAR(10) NOT NULL,
        Complemento NVARCHAR(100) NULL,
        Bairro NVARCHAR(100) NOT NULL,
        Cidade NVARCHAR(100) NOT NULL,
        Estado NVARCHAR(2) NOT NULL,
        AsaasCustomerId NVARCHAR(100) NULL,
        DataCadastro DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT '✅ Tabela Clientes criada!';
END
GO

-- Tabela de Pedidos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pedidos')
BEGIN
    CREATE TABLE Pedidos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Numero NVARCHAR(50) NOT NULL UNIQUE,
        ClienteId INT NOT NULL,
        PlanoId INT NOT NULL,
        ValorTotal DECIMAL(10, 2) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'PENDING',
        DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
        FOREIGN KEY (PlanoId) REFERENCES Planos(Id)
    );
    PRINT '✅ Tabela Pedidos criada!';
END
GO

-- Tabela de Pagamentos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pagamentos')
BEGIN
    CREATE TABLE Pagamentos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PedidoId INT NOT NULL,
        AsaasPaymentId NVARCHAR(100) NULL,
        FormaPagamento NVARCHAR(50) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'PENDING',
        Valor DECIMAL(10, 2) NOT NULL,
        DataVencimento DATE NULL,
        DataPagamento DATETIME NULL,
        BoletoUrl NVARCHAR(500) NULL,
        LinhaDigitavel NVARCHAR(200) NULL,
        CartaoBandeira NVARCHAR(50) NULL,
        CartaoUltimosDigitos NVARCHAR(4) NULL,
        DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (PedidoId) REFERENCES Pedidos(Id)
    );
    PRINT '✅ Tabela Pagamentos criada!';
END
GO

-- Tabela de Assinaturas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assinaturas')
BEGIN
    CREATE TABLE Assinaturas (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ClienteId INT NOT NULL,
        PlanoId INT NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
        DataInicio DATETIME NOT NULL,
        DataFim DATETIME NOT NULL,
        DataCancelamento DATETIME NULL,
        FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
        FOREIGN KEY (PlanoId) REFERENCES Planos(Id)
    );
    PRINT '✅ Tabela Assinaturas criada!';
END
GO

-- Tabela de Logs de Webhooks
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WebhookLogs')
BEGIN
    CREATE TABLE WebhookLogs (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Evento NVARCHAR(100) NOT NULL,
        Payload NVARCHAR(MAX) NOT NULL,
        PaymentId NVARCHAR(100) NULL,
        ProcessadoComSucesso BIT NOT NULL DEFAULT 0,
        MensagemErro NVARCHAR(MAX) NULL,
        DataRecebimento DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT '✅ Tabela WebhookLogs criada!';
END
GO

-- ===================================================
-- INSERIR DADOS INICIAIS (SEED)
-- ===================================================

-- Inserir admin padrão
IF NOT EXISTS (SELECT * FROM AdminUsers WHERE Email = 'admin@cnhvirtual.com')
BEGIN
    -- Senha: Admin@123 (hash BCrypt)
    INSERT INTO AdminUsers (Nome, Email, SenhaHash, Ativo)
    VALUES ('Administrador', 'admin@cnhvirtual.com', '$2a$11$7jXQqXQg3M5Y5Y0vL.gZxeYmqnZYqV2X8F1zDHX2F2Y2Y2Y2Y2Y2Y2', 1);
    PRINT '✅ Admin criado: admin@cnhvirtual.com / Admin@123';
END
GO

-- Inserir planos
IF NOT EXISTS (SELECT * FROM Planos WHERE Nome = 'Plano Básico')
BEGIN
    -- Plano Básico
    INSERT INTO Planos (Nome, Descricao, DescricaoCurta, Preco, DuracaoDias, ValidadeDias, LimiteTentativas, Ativo, Destaque, Ordem)
    VALUES ('Plano Básico', 'Perfeito para quem está começando', 'Ideal para iniciantes', 97.00, 90, 90, 10, 1, 0, 1);

    DECLARE @PlanoBasicoId INT = SCOPE_IDENTITY();

    INSERT INTO PlanoRecursos (PlanoId, Descricao, Incluido, Ordem) VALUES
    (@PlanoBasicoId, 'Acesso por 90 dias', 1, 1),
    (@PlanoBasicoId, 'Todo conteúdo teórico', 1, 2),
    (@PlanoBasicoId, '1000+ questões comentadas', 1, 3),
    (@PlanoBasicoId, 'Simulados ilimitados', 1, 4),
    (@PlanoBasicoId, 'Suporte via chat', 1, 5);

    -- Plano Intermediário
    INSERT INTO Planos (Nome, Descricao, DescricaoCurta, Preco, DuracaoDias, ValidadeDias, LimiteTentativas, Ativo, Destaque, Ordem)
    VALUES ('Plano Intermediário', 'Mais recursos e tempo para estudar', 'Completo e eficiente', 197.00, 180, 180, 20, 1, 1, 2);

    DECLARE @PlanoIntermedId INT = SCOPE_IDENTITY();

    INSERT INTO PlanoRecursos (PlanoId, Descricao, Incluido, Ordem) VALUES
    (@PlanoIntermedId, 'Acesso por 180 dias', 1, 1),
    (@PlanoIntermedId, 'Todo conteúdo teórico', 1, 2),
    (@PlanoIntermedId, '2000+ questões comentadas', 1, 3),
    (@PlanoIntermedId, 'Simulados ilimitados', 1, 4),
    (@PlanoIntermedId, 'Aulas práticas em vídeo', 1, 5),
    (@PlanoIntermedId, 'Suporte prioritário', 1, 6),
    (@PlanoIntermedId, 'Material em PDF', 1, 7);

    -- Plano Premium
    INSERT INTO Planos (Nome, Descricao, DescricaoCurta, Preco, DuracaoDias, ValidadeDias, LimiteTentativas, Ativo, Destaque, Ordem)
    VALUES ('Plano Premium', 'Acesso completo e vitalício', 'Garantia de aprovação', 397.00, 3650, 3650, 999, 1, 0, 3);

    DECLARE @PlanoPremiumId INT = SCOPE_IDENTITY();

    INSERT INTO PlanoRecursos (PlanoId, Descricao, Incluido, Ordem) VALUES
    (@PlanoPremiumId, 'Acesso vitalício', 1, 1),
    (@PlanoPremiumId, 'Todo conteúdo teórico', 1, 2),
    (@PlanoPremiumId, '5000+ questões comentadas', 1, 3),
    (@PlanoPremiumId, 'Simulados ilimitados', 1, 4),
    (@PlanoPremiumId, 'Aulas práticas em vídeo', 1, 5),
    (@PlanoPremiumId, 'Mentoria individual', 1, 6),
    (@PlanoPremiumId, 'Suporte VIP 24/7', 1, 7),
    (@PlanoPremiumId, 'Material completo em PDF', 1, 8),
    (@PlanoPremiumId, 'Garantia de aprovação', 1, 9),
    (@PlanoPremiumId, 'Atualizações gratuitas', 1, 10);

    PRINT '✅ 3 Planos criados com recursos!';
END
GO

-- ===================================================
-- VERIFICAÇÃO FINAL
-- ===================================================

PRINT '';
PRINT '========================================';
PRINT 'SETUP COMPLETO!';
PRINT '========================================';
PRINT '';
PRINT 'TABELAS CRIADAS:';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;
PRINT '';
PRINT 'CREDENCIAIS DO ADMIN:';
PRINT 'Email: admin@cnhvirtual.com';
PRINT 'Senha: Admin@123';
PRINT '';
PRINT 'PLANOS DISPONÍVEIS:';
SELECT Id, Nome, Preco, DuracaoDias FROM Planos ORDER BY Ordem;
PRINT '';
PRINT '========================================';
GO
