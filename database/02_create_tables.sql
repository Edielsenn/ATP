-- Script de criação das tabelas do PNHDigitalDB
-- Execute após criar o banco de dados

USE PNHDigitalDB;
GO

-- Tabela de Administradores
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AdminUsers')
BEGIN
    CREATE TABLE AdminUsers (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        SenhaHash NVARCHAR(255) NOT NULL,
        Ativo BIT DEFAULT 1,
        DataCriacao DATETIME DEFAULT GETDATE(),
        UltimoAcesso DATETIME NULL
    );
    PRINT 'Tabela AdminUsers criada com sucesso!';
END
GO

-- Tabela de Planos/Cursos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Planos')
BEGIN
    CREATE TABLE Planos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(200) NOT NULL,
        Descricao NVARCHAR(MAX) NOT NULL,
        DescricaoCurta NVARCHAR(500) NULL,
        Preco DECIMAL(10,2) NOT NULL,
        PrecoPromocional DECIMAL(10,2) NULL,
        DuracaoDias INT NOT NULL, -- Duração do acesso em dias
        ValidadeDias INT NULL, -- Validade após ativação
        LimiteAlunos INT NULL, -- Limite de alunos por compra
        LimiteTentativas INT NULL, -- Limite de tentativas de prova
        Ativo BIT DEFAULT 1,
        Destaque BIT DEFAULT 0, -- Se é plano em destaque
        Ordem INT DEFAULT 0, -- Ordem de exibição
        DataCriacao DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME DEFAULT GETDATE()
    );
    PRINT 'Tabela Planos criada com sucesso!';
END
GO

-- Tabela de Recursos dos Planos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PlanoRecursos')
BEGIN
    CREATE TABLE PlanoRecursos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PlanoId INT NOT NULL,
        Descricao NVARCHAR(500) NOT NULL,
        Incluido BIT DEFAULT 1, -- Se está incluído ou é uma limitação
        Ordem INT DEFAULT 0,
        FOREIGN KEY (PlanoId) REFERENCES Planos(Id) ON DELETE CASCADE
    );
    PRINT 'Tabela PlanoRecursos criada com sucesso!';
END
GO

-- Tabela de Clientes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(200) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        Telefone NVARCHAR(20) NULL,
        CPF NVARCHAR(14) NULL,
        DataNascimento DATE NULL,
        -- Endereço
        CEP NVARCHAR(10) NULL,
        Endereco NVARCHAR(300) NULL,
        Numero NVARCHAR(20) NULL,
        Complemento NVARCHAR(100) NULL,
        Bairro NVARCHAR(100) NULL,
        Cidade NVARCHAR(100) NULL,
        Estado NVARCHAR(2) NULL,
        -- Controle
        DataCriacao DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME DEFAULT GETDATE()
    );
    PRINT 'Tabela Clientes criada com sucesso!';
END
GO

-- Tabela de Pedidos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pedidos')
BEGIN
    CREATE TABLE Pedidos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ClienteId INT NOT NULL,
        PlanoId INT NOT NULL,
        Numero NVARCHAR(50) NOT NULL UNIQUE, -- Número único do pedido
        ValorTotal DECIMAL(10,2) NOT NULL,
        ValorDesconto DECIMAL(10,2) DEFAULT 0,
        ValorFinal DECIMAL(10,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL, -- PENDING, CONFIRMED, CANCELLED
        DataPedido DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
        FOREIGN KEY (PlanoId) REFERENCES Planos(Id)
    );
    PRINT 'Tabela Pedidos criada com sucesso!';
END
GO

-- Tabela de Pagamentos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pagamentos')
BEGIN
    CREATE TABLE Pagamentos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PedidoId INT NOT NULL,
        AsaasPaymentId NVARCHAR(100) NULL, -- ID do pagamento no ASAAS
        FormaPagamento NVARCHAR(50) NOT NULL, -- BOLETO, CREDIT_CARD, PIX
        Status NVARCHAR(50) NOT NULL, -- PENDING, CONFIRMED, RECEIVED, OVERDUE, REFUNDED, CANCELLED
        Valor DECIMAL(10,2) NOT NULL,
        ValorRecebido DECIMAL(10,2) NULL,
        DataVencimento DATE NULL,
        DataPagamento DATETIME NULL,
        DataConfirmacao DATETIME NULL,
        -- Dados do Boleto
        BoletoUrl NVARCHAR(500) NULL,
        LinhaDigitavel NVARCHAR(200) NULL,
        CodigoBarras NVARCHAR(200) NULL,
        -- Dados do Cartão
        CartaoBandeira NVARCHAR(50) NULL,
        CartaoUltimosDigitos NVARCHAR(4) NULL,
        -- Controle
        Observacoes NVARCHAR(MAX) NULL,
        DataCriacao DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (PedidoId) REFERENCES Pedidos(Id)
    );
    PRINT 'Tabela Pagamentos criada com sucesso!';
END
GO

-- Tabela de Assinaturas/Acessos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assinaturas')
BEGIN
    CREATE TABLE Assinaturas (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ClienteId INT NOT NULL,
        PlanoId INT NOT NULL,
        PedidoId INT NOT NULL,
        Status NVARCHAR(50) NOT NULL, -- ACTIVE, EXPIRED, CANCELLED
        DataInicio DATE NOT NULL,
        DataFim DATE NOT NULL,
        DataCancelamento DATE NULL,
        TentativasUsadas INT DEFAULT 0,
        Observacoes NVARCHAR(MAX) NULL,
        DataCriacao DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
        FOREIGN KEY (PlanoId) REFERENCES Planos(Id),
        FOREIGN KEY (PedidoId) REFERENCES Pedidos(Id)
    );
    PRINT 'Tabela Assinaturas criada com sucesso!';
END
GO

-- Tabela de Logs de Webhook (ASAAS)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WebhookLogs')
BEGIN
    CREATE TABLE WebhookLogs (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Evento NVARCHAR(100) NOT NULL,
        PayloadJson NVARCHAR(MAX) NOT NULL,
        ProcessadoComSucesso BIT DEFAULT 0,
        MensagemErro NVARCHAR(MAX) NULL,
        DataRecebimento DATETIME DEFAULT GETDATE(),
        DataProcessamento DATETIME NULL
    );
    PRINT 'Tabela WebhookLogs criada com sucesso!';
END
GO

-- Criar índices para melhorar performance
CREATE INDEX IX_Clientes_Email ON Clientes(Email);
CREATE INDEX IX_Pedidos_Numero ON Pedidos(Numero);
CREATE INDEX IX_Pedidos_Status ON Pedidos(Status);
CREATE INDEX IX_Pagamentos_AsaasPaymentId ON Pagamentos(AsaasPaymentId);
CREATE INDEX IX_Pagamentos_Status ON Pagamentos(Status);
CREATE INDEX IX_Assinaturas_Status ON Assinaturas(Status);
CREATE INDEX IX_Assinaturas_DataFim ON Assinaturas(DataFim);
GO

PRINT 'Todos os índices criados com sucesso!';
PRINT 'Estrutura do banco de dados PNHDigitalDB criada com sucesso!';
