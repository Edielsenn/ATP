# CNH Virtual - Plataforma de Vendas de Cursos para CNH

Sistema completo de vendas de cursos para habilitação (CNH) com landing page moderna, API backend, integração com ASAAS (pagamentos) e painel administrativo.

## 📁 Estrutura do Projeto

```
CNHVirtual/
├── database/                    # Scripts SQL do banco de dados
│   ├── 01_create_database.sql
│   ├── 02_create_tables.sql
│   ├── 03_seed_data.sql
│   └── README.md
├── CNHVirtual/                  # Frontend Blazor (.NET 10)
│   ├── Components/
│   │   ├── Layout/
│   │   │   ├── HeaderNav.razor
│   │   │   ├── FooterSection.razor
│   │   │   ├── Hero.razor
│   │   │   ├── Features.razor
│   │   │   ├── PlansSection.razor
│   │   │   ├── PlanCard.razor
│   │   │   ├── HowItWorks.razor
│   │   │   ├── Testimonials.razor
│   │   │   ├── FAQ.razor
│   │   │   └── CTA.razor
│   │   └── Pages/
│   │       └── Home.razor
│   ├── wwwroot/
│   │   └── css/
│   │       └── app.css
│   └── Program.cs
└── APICNHVirtual/              # API Backend (.NET 10)
    ├── Controllers/            # (A criar)
    ├── Models/                 # (A criar)
    ├── Services/               # (A criar)
    └── Program.cs
```

## 🗄️ Banco de Dados - PNHDigitalDB

### Configuração

1. Abra o SQL Server Management Studio
2. Execute os scripts na ordem:

```sql
-- 1. Criar o banco de dados
USE master;
GO
-- Execute: database/01_create_database.sql

-- 2. Criar as tabelas
USE PNHDigitalDB;
GO
-- Execute: database/02_create_tables.sql

-- 3. Inserir dados iniciais
-- Execute: database/03_seed_data.sql
```

### Credenciais Padrão

- **Email Admin:** admin@cnhvirtual.com
- **Senha Admin:** Admin@123
- ⚠️ **ALTERE A SENHA APÓS O PRIMEIRO LOGIN!**

### Tabelas Criadas

- **AdminUsers** - Usuários administradores
- **Planos** - Planos/cursos disponíveis
- **PlanoRecursos** - Recursos de cada plano
- **Clientes** - Dados dos clientes
- **Pedidos** - Pedidos realizados
- **Pagamentos** - Pagamentos via ASAAS
- **Assinaturas** - Controle de acesso aos cursos
- **WebhookLogs** - Logs de webhooks do ASAAS

### Connection String

```
Server=localhost;Database=PNHDigitalDB;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=True;
```

## 🎨 Frontend - Blazor (CNHVirtual)

### Tecnologias

- ASP.NET Core 10.0
- Blazor Server
- CSS customizado (baseado no modelo fornecido)

### Componentes Criados

✅ **HeaderNav** - Cabeçalho fixo com navegação
✅ **Hero** - Seção principal com estatísticas
✅ **Features** - 6 benefícios principais
✅ **PlansSection** - Exibição de planos (integra com API)
✅ **PlanCard** - Card individual de plano
✅ **HowItWorks** - 6 passos do processo
✅ **Testimonials** - 6 depoimentos de alunos
✅ **FAQ** - Perguntas frequentes (accordion)
✅ **CTA** - Call-to-action final
✅ **FooterSection** - Rodapé completo

### Como Executar

```bash
cd CNHVirtual
dotnet restore
dotnet run
```

Acesse: https://localhost:5001

### Configuração

Edite `appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7000"
  }
}
```

## 🔧 API Backend - APICNHVirtual

### Status Atual

✅ Projeto criado
✅ Pacotes instalados:
- Entity Framework Core 10.0.2
- SQL Server Provider
- JWT Authentication (instalando)
- BCrypt (instalando)
- Swagger/OpenAPI

### Próximos Passos - API

1. **Criar Models** (Models/)
   - Plano.cs
   - Cliente.cs
   - Pedido.cs
   - Pagamento.cs
   - AdminUser.cs

2. **Criar DbContext** (Data/)
   - ApplicationDbContext.cs

3. **Criar Services** (Services/)
   - AsaasService.cs (integração ASAAS com split)
   - PagamentoService.cs
   - AuthService.cs

4. **Criar Controllers** (Controllers/)
   - PlanosController.cs
   - ClientesController.cs
   - PagamentosController.cs
   - WebhookController.cs (ASAAS)
   - AdminController.cs

5. **Configurar** (Program.cs)
   - CORS
   - Entity Framework
   - JWT Authentication
   - Swagger

### Configuração ASAAS

Edite `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PNHDigitalDB;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=True;"
  },
  "Asaas": {
    "ApiKey": "SUA_CHAVE_ASAAS",
    "Environment": "sandbox",
    "ApiUrl": "https://sandbox.asaas.com/api/v3",
    "SplitWalletId": "ID_CARTEIRA_SPLIT"
  },
  "Jwt": {
    "Secret": "SUA_CHAVE_JWT_SUPER_SECRETA_AQUI",
    "Issuer": "CNHVirtualAPI",
    "Audience": "CNHVirtualApp",
    "ExpirationHours": 24
  }
}
```

## 💳 Integração ASAAS

### Recursos Suportados

- ✅ Pagamento por Boleto
- ✅ Pagamento por Cartão de Crédito
- ✅ Split de Pagamentos (configurável)
- ✅ Webhooks para notificações
- ⏳ PIX (futuro)

### Split de Pagamentos

Configure o split no `appsettings.json`:

```json
{
  "Asaas": {
    "SplitWalletId": "ID_DA_SUBCONTA_ASAAS",
    "SplitPercentage": 10.0
  }
}
```

### Webhooks

Configure no painel do ASAAS para apontar para:

```
POST https://seu-dominio.com/api/webhook/asaas
```

Eventos suportados:
- PAYMENT_CREATED
- PAYMENT_CONFIRMED
- PAYMENT_RECEIVED
- PAYMENT_OVERDUE
- PAYMENT_DELETED
- PAYMENT_REFUNDED

## 📊 Painel Administrativo

### Status

⏳ **A ser desenvolvido**

### Funcionalidades Planejadas

- Login com JWT
- Dashboard de vendas
- Gerenciamento de planos
- Visualização de pedidos
- Controle de pagamentos
- Listagem de clientes
- Relatórios e estatísticas
- Gerenciamento de assinaturas

## 🚀 Deploy

### Frontend Blazor

```bash
cd CNHVirtual
dotnet publish -c Release
```

Hospedar em:
- Azure App Service
- IIS (Windows Server)
- Docker

### API Backend

```bash
cd APICNHVirtual
dotnet publish -c Release
```

Hospedar em:
- Azure App Service
- IIS (Windows Server)
- Docker

### Banco de Dados

- SQL Server local
- Azure SQL Database
- SQL Server em VM

## 📝 TODO List

### Prioridade Alta

- [ ] Completar API Backend (Models, Controllers, Services)
- [ ] Implementar integração completa com ASAAS
- [ ] Criar página de Checkout no Blazor
- [ ] Implementar processamento de pagamentos
- [ ] Configurar webhooks do ASAAS

### Prioridade Média

- [ ] Criar painel administrativo
- [ ] Implementar autenticação JWT
- [ ] Adicionar gerenciamento de planos
- [ ] Criar relatórios de vendas

### Prioridade Baixa

- [ ] Adicionar suporte a PIX
- [ ] Implementar sistema de cupons de desconto
- [ ] Criar área do aluno
- [ ] Adicionar notificações por email

## 🔒 Segurança

### Checklist

- [ ] Trocar senha padrão do admin
- [ ] Configurar chave JWT forte
- [ ] Usar HTTPS em produção
- [ ] Validar todos os inputs
- [ ] Sanitizar dados do banco
- [ ] Implementar rate limiting
- [ ] Configurar CORS adequadamente
- [ ] Proteger endpoints sensíveis
- [ ] Criptografar dados sensíveis
- [ ] Fazer backup regular do banco

## 📞 Suporte

Para dúvidas ou suporte:

- Email: contato@cnhvirtual.com
- Telefone: (11) 99999-9999

## 📄 Licença

© 2026 CNH Virtual. Todos os direitos reservados.
