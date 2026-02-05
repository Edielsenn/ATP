-- Script de dados iniciais (seed) do PNHDigitalDB
-- Execute após criar as tabelas

USE PNHDigitalDB;
GO

-- Inserir usuário administrador padrão
-- Senha padrão: Admin@123 (você deve alterar após o primeiro login)
-- Hash gerado com bcrypt para 'Admin@123'
IF NOT EXISTS (SELECT * FROM AdminUsers WHERE Email = 'admin@pnhdigital.com')
BEGIN
    INSERT INTO AdminUsers (Nome, Email, SenhaHash, Ativo)
    VALUES ('Administrador', 'admin@pnhdigital.com', '$2b$10$rGHvEWvQZ5X8N.BkzKqD0OSvGYqKp5PqQxCw7tqKLMJ8xYZzKqN8S', 1);
    PRINT 'Usuário administrador padrão criado!';
    PRINT 'Email: admin@pnhdigital.com';
    PRINT 'Senha: Admin@123 (ALTERE APÓS O PRIMEIRO LOGIN!)';
END
GO

-- Inserir planos de exemplo
IF NOT EXISTS (SELECT * FROM Planos WHERE Nome = 'Plano Básico')
BEGIN
    INSERT INTO Planos (Nome, Descricao, DescricaoCurta, Preco, DuracaoDias, ValidadeDias, LimiteTentativas, Ativo, Ordem)
    VALUES
    (
        'Plano Básico',
        'Perfeito para quem está começando sua jornada para conquista da CNH. Inclui todo conteúdo teórico necessário para aprovação.',
        'Ideal para iniciantes',
        97.00,
        90, -- 90 dias de acesso
        180, -- Válido por 180 dias após ativação
        3, -- 3 tentativas de prova
        1,
        1
    ),
    (
        'Plano Intermediário',
        'O plano mais popular! Além de todo conteúdo teórico, você tem acesso a simulados ilimitados e suporte prioritário.',
        'Mais vendido',
        197.00,
        180, -- 180 dias de acesso
        365, -- Válido por 1 ano após ativação
        5, -- 5 tentativas de prova
        1,
        2
    ),
    (
        'Plano Premium',
        'A escolha perfeita para quem quer garantir a aprovação! Acesso vitalício, simulados ilimitados, aulas práticas em vídeo e suporte VIP.',
        'Garantia de aprovação',
        397.00,
        3650, -- 10 anos de acesso (vitalício)
        NULL, -- Sem validade
        NULL, -- Tentativas ilimitadas
        1,
        3
    );
    PRINT 'Planos de exemplo criados!';
END
GO

-- Inserir recursos do Plano Básico
DECLARE @PlanoBasicoId INT = (SELECT Id FROM Planos WHERE Nome = 'Plano Básico');
IF @PlanoBasicoId IS NOT NULL AND NOT EXISTS (SELECT * FROM PlanoRecursos WHERE PlanoId = @PlanoBasicoId)
BEGIN
    INSERT INTO PlanoRecursos (PlanoId, Descricao, Incluido, Ordem)
    VALUES
    (@PlanoBasicoId, 'Acesso por 90 dias', 1, 1),
    (@PlanoBasicoId, 'Todo conteúdo teórico', 1, 2),
    (@PlanoBasicoId, 'Questões comentadas', 1, 3),
    (@PlanoBasicoId, '3 tentativas de simulado', 1, 4),
    (@PlanoBasicoId, 'Suporte por email', 1, 5),
    (@PlanoBasicoId, 'Certificado digital', 1, 6);
    PRINT 'Recursos do Plano Básico adicionados!';
END
GO

-- Inserir recursos do Plano Intermediário
DECLARE @PlanoIntermediarioId INT = (SELECT Id FROM Planos WHERE Nome = 'Plano Intermediário');
IF @PlanoIntermediarioId IS NOT NULL AND NOT EXISTS (SELECT * FROM PlanoRecursos WHERE PlanoId = @PlanoIntermediarioId)
BEGIN
    INSERT INTO PlanoRecursos (PlanoId, Descricao, Incluido, Ordem)
    VALUES
    (@PlanoIntermediarioId, 'Acesso por 180 dias', 1, 1),
    (@PlanoIntermediarioId, 'Todo conteúdo teórico', 1, 2),
    (@PlanoIntermediarioId, 'Questões comentadas', 1, 3),
    (@PlanoIntermediarioId, 'Simulados ilimitados', 1, 4),
    (@PlanoIntermediarioId, 'Estatísticas de desempenho', 1, 5),
    (@PlanoIntermediarioId, 'Suporte prioritário', 1, 6),
    (@PlanoIntermediarioId, 'Certificado digital', 1, 7),
    (@PlanoIntermediarioId, 'Material em PDF', 1, 8);
    PRINT 'Recursos do Plano Intermediário adicionados!';
END
GO

-- Inserir recursos do Plano Premium
DECLARE @PlanoPremiumId INT = (SELECT Id FROM Planos WHERE Nome = 'Plano Premium');
IF @PlanoPremiumId IS NOT NULL AND NOT EXISTS (SELECT * FROM PlanoRecursos WHERE PlanoId = @PlanoPremiumId)
BEGIN
    INSERT INTO PlanoRecursos (PlanoId, Descricao, Incluido, Ordem)
    VALUES
    (@PlanoPremiumId, 'Acesso vitalício', 1, 1),
    (@PlanoPremiumId, 'Todo conteúdo teórico', 1, 2),
    (@PlanoPremiumId, 'Questões comentadas', 1, 3),
    (@PlanoPremiumId, 'Simulados ilimitados', 1, 4),
    (@PlanoPremiumId, 'Aulas práticas em vídeo', 1, 5),
    (@PlanoPremiumId, 'Estatísticas avançadas', 1, 6),
    (@PlanoPremiumId, 'Suporte VIP 24/7', 1, 7),
    (@PlanoPremiumId, 'Certificado digital', 1, 8),
    (@PlanoPremiumId, 'Material completo em PDF', 1, 9),
    (@PlanoPremiumId, 'Garantia de aprovação', 1, 10),
    (@PlanoPremiumId, 'Atualizações gratuitas', 1, 11);
    PRINT 'Recursos do Plano Premium adicionados!';
END
GO

-- Marcar Plano Intermediário como destaque
UPDATE Planos SET Destaque = 1 WHERE Nome = 'Plano Intermediário';
GO

PRINT 'Dados iniciais inseridos com sucesso!';
PRINT '';
PRINT '=== RESUMO DA CONFIGURAÇÃO ===';
PRINT 'Banco de dados: PNHDigitalDB';
PRINT 'Admin Email: admin@pnhdigital.com';
PRINT 'Admin Senha: Admin@123';
PRINT 'Planos criados: 3 (Básico, Intermediário, Premium)';
PRINT '';
PRINT 'IMPORTANTE: Altere a senha do administrador após o primeiro login!';
