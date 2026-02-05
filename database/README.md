# Banco de Dados PNHDigitalDB

## Estrutura do Banco de Dados

Este diretório contém os scripts SQL para criar e configurar o banco de dados **PNHDigitalDB** no SQL Server.

## Scripts Disponíveis

Execute os scripts na seguinte ordem:

### 1. `01_create_database.sql`
Cria o banco de dados PNHDigitalDB.

```sql
sqlcmd -S localhost -U sa -P SuaSenha -i 01_create_database.sql
```

### 2. `02_create_tables.sql`
Cria todas as tabelas necessárias:
- **AdminUsers**: Usuários administradores do painel
- **Planos**: Planos/cursos disponíveis para venda
- **PlanoRecursos**: Recursos e benefícios de cada plano
- **Clientes**: Dados dos clientes que compraram
- **Pedidos**: Pedidos realizados
- **Pagamentos**: Pagamentos processados via ASAAS
- **Assinaturas**: Controle de acesso dos clientes aos planos
- **WebhookLogs**: Logs dos webhooks recebidos do ASAAS

```sql
sqlcmd -S localhost -U sa -P SuaSenha -i 02_create_tables.sql
```

### 3. `03_seed_data.sql`
Insere dados iniciais:
- Usuário administrador padrão
- 3 planos de exemplo (Básico, Intermediário, Premium)
- Recursos de cada plano

```sql
sqlcmd -S localhost -U sa -P SuaSenha -i 03_seed_data.sql
```

## Credenciais Padrão do Administrador

Após executar o script de seed:

- **Email**: admin@pnhdigital.com
- **Senha**: Admin@123

⚠️ **IMPORTANTE**: Altere esta senha após o primeiro login!

## Estrutura das Tabelas

### Planos
Cada plano contém:
- Nome, descrição e preços
- Duração do acesso (em dias)
- Validade após ativação
- Limites de alunos e tentativas
- Status de ativo/inativo e destaque

### Pagamentos
Integração com ASAAS suportando:
- Boleto bancário
- Cartão de crédito
- PIX (futuro)

### Status de Pagamento
- **PENDING**: Aguardando pagamento
- **CONFIRMED**: Pagamento confirmado
- **RECEIVED**: Pagamento recebido
- **OVERDUE**: Vencido
- **REFUNDED**: Estornado
- **CANCELLED**: Cancelado

## Connection String de Exemplo

```
Server=localhost;Database=PNHDigitalDB;User Id=sa;Password=SuaSenha;TrustServerCertificate=True;
```

## Backup e Manutenção

Para fazer backup do banco:
```sql
BACKUP DATABASE PNHDigitalDB
TO DISK = 'C:\Backup\PNHDigitalDB.bak'
WITH FORMAT, MEDIANAME = 'PNHDigitalBackup', NAME = 'Full Backup of PNHDigitalDB';
```
