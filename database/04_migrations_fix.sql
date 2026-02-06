-- =====================================================
-- MIGRAÇÕES E CORREÇÕES DO BANCO DE DADOS
-- CNH Virtual - Correções aplicadas durante desenvolvimento
-- =====================================================

USE PNHDigitalDB;
GO

-- =====================================================
-- 1. ADICIONAR COLUNA SenhaHash NA TABELA Clientes
-- =====================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'SenhaHash')
BEGIN
    ALTER TABLE Clientes ADD SenhaHash NVARCHAR(MAX) NULL;
    PRINT '✓ Column SenhaHash added to Clientes';
END
ELSE
BEGIN
    PRINT '- Column SenhaHash already exists in Clientes';
END
GO

-- =====================================================
-- 2. CRIAR TABELA EmailTemplates
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmailTemplates')
BEGIN
    CREATE TABLE EmailTemplates (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(MAX) NOT NULL,
        Codigo NVARCHAR(MAX) NOT NULL,
        Assunto NVARCHAR(MAX) NOT NULL,
        CorpoHtml NVARCHAR(MAX) NOT NULL,
        CorpoTexto NVARCHAR(MAX) NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME2 NULL
    );
    PRINT '✓ Table EmailTemplates created';
END
ELSE
BEGIN
    PRINT '- Table EmailTemplates already exists';
END
GO

-- =====================================================
-- 3. ADICIONAR COLUNAS FALTANTES NA TABELA Clientes
-- =====================================================
-- DataCriacao
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'DataCriacao')
BEGIN
    ALTER TABLE Clientes ADD DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE();
    PRINT '✓ Column DataCriacao added to Clientes';
END

-- DataAtualizacao
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'DataAtualizacao')
BEGIN
    ALTER TABLE Clientes ADD DataAtualizacao DATETIME2 NOT NULL DEFAULT GETDATE();
    PRINT '✓ Column DataAtualizacao added to Clientes';
END

-- Endereco
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Endereco')
BEGIN
    ALTER TABLE Clientes ADD Endereco NVARCHAR(MAX) NULL;
    PRINT '✓ Column Endereco added to Clientes';
END
GO

-- =====================================================
-- 4. TORNAR COLUNAS NULLABLE NA TABELA Clientes
-- =====================================================
-- DataNascimento
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'DataNascimento' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE Clientes ALTER COLUMN DataNascimento DATETIME2 NULL;
    PRINT '✓ Column DataNascimento changed to nullable';
END

-- Telefone
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Telefone' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE Clientes ALTER COLUMN Telefone NVARCHAR(MAX) NULL;
    PRINT '✓ Column Telefone changed to nullable';
END

-- CPF
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'CPF' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE Clientes ALTER COLUMN CPF NVARCHAR(MAX) NULL;
    PRINT '✓ Column CPF changed to nullable';
END

-- CEP
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'CEP' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE Clientes ALTER COLUMN CEP NVARCHAR(MAX) NULL;
    PRINT '✓ Column CEP changed to nullable';
END

-- Logradouro
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Logradouro' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE Clientes ALTER COLUMN Logradouro NVARCHAR(MAX) NULL;
    PRINT '✓ Column Logradouro changed to nullable';
END

-- Numero
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Numero' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE Clientes ALTER COLUMN Numero NVARCHAR(MAX) NULL;
    PRINT '✓ Column Numero changed to nullable';
END

-- Bairro
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Bairro' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE Clientes ALTER COLUMN Bairro NVARCHAR(MAX) NULL;
    PRINT '✓ Column Bairro changed to nullable';
END

-- Cidade
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Cidade' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE Clientes ALTER COLUMN Cidade NVARCHAR(MAX) NULL;
    PRINT '✓ Column Cidade changed to nullable';
END

-- Estado
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Estado' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE Clientes ALTER COLUMN Estado NVARCHAR(MAX) NULL;
    PRINT '✓ Column Estado changed to nullable';
END
GO

-- =====================================================
-- 5. ADICIONAR COLUNAS FALTANTES NA TABELA Pedidos
-- =====================================================
-- DataPedido
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Pedidos' AND COLUMN_NAME = 'DataPedido')
BEGIN
    ALTER TABLE Pedidos ADD DataPedido DATETIME2 NOT NULL DEFAULT GETDATE();
    PRINT '✓ Column DataPedido added to Pedidos';
END

-- DataAtualizacao
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Pedidos' AND COLUMN_NAME = 'DataAtualizacao')
BEGIN
    ALTER TABLE Pedidos ADD DataAtualizacao DATETIME2 NOT NULL DEFAULT GETDATE();
    PRINT '✓ Column DataAtualizacao added to Pedidos';
END

-- ValorDesconto
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Pedidos' AND COLUMN_NAME = 'ValorDesconto')
BEGIN
    ALTER TABLE Pedidos ADD ValorDesconto DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT '✓ Column ValorDesconto added to Pedidos';
END

-- ValorFinal
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Pedidos' AND COLUMN_NAME = 'ValorFinal')
BEGIN
    ALTER TABLE Pedidos ADD ValorFinal DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT '✓ Column ValorFinal added to Pedidos';
END
GO

-- =====================================================
-- 6. ADICIONAR COLUNAS FALTANTES NA TABELA Pagamentos
-- =====================================================
-- CodigoBarras
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Pagamentos' AND COLUMN_NAME = 'CodigoBarras')
BEGIN
    ALTER TABLE Pagamentos ADD CodigoBarras NVARCHAR(MAX) NULL;
    PRINT '✓ Column CodigoBarras added to Pagamentos';
END

-- DataAtualizacao
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Pagamentos' AND COLUMN_NAME = 'DataAtualizacao')
BEGIN
    ALTER TABLE Pagamentos ADD DataAtualizacao DATETIME2 NULL;
    PRINT '✓ Column DataAtualizacao added to Pagamentos';
END

-- DataConfirmacao
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Pagamentos' AND COLUMN_NAME = 'DataConfirmacao')
BEGIN
    ALTER TABLE Pagamentos ADD DataConfirmacao DATETIME2 NULL;
    PRINT '✓ Column DataConfirmacao added to Pagamentos';
END

-- Observacoes
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Pagamentos' AND COLUMN_NAME = 'Observacoes')
BEGIN
    ALTER TABLE Pagamentos ADD Observacoes NVARCHAR(MAX) NULL;
    PRINT '✓ Column Observacoes added to Pagamentos';
END

-- ValorRecebido
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Pagamentos' AND COLUMN_NAME = 'ValorRecebido')
BEGIN
    ALTER TABLE Pagamentos ADD ValorRecebido DECIMAL(10,2) NULL;
    PRINT '✓ Column ValorRecebido added to Pagamentos';
END
GO

-- =====================================================
-- 7. ADICIONAR PedidoId NA TABELA Assinaturas
-- =====================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Assinaturas' AND COLUMN_NAME = 'PedidoId')
BEGIN
    ALTER TABLE Assinaturas ADD PedidoId INT NOT NULL DEFAULT 0;

    -- Add foreign key constraint
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assinaturas_Pedidos_PedidoId')
    BEGIN
        ALTER TABLE Assinaturas
        ADD CONSTRAINT FK_Assinaturas_Pedidos_PedidoId
        FOREIGN KEY (PedidoId) REFERENCES Pedidos(Id);
    END

    PRINT '✓ Column PedidoId added to Assinaturas with FK constraint';
END
ELSE
BEGIN
    PRINT '- Column PedidoId already exists in Assinaturas';
END
GO

PRINT '';
PRINT '=====================================================';
PRINT 'TODAS AS MIGRAÇÕES FORAM APLICADAS COM SUCESSO!';
PRINT '=====================================================';
GO
