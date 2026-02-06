# CNH Virtual - Sistema de Gestão de Cursos

Sistema completo para gestão de cursos preparatórios para CNH, desenvolvido com .NET 10.0, Blazor e integração com gateway de pagamento ASAAS.

## 🚀 Tecnologias

### Backend (API)
- **.NET 10.0** - Framework principal
- **ASP.NET Core Web API** - API RESTful
- **Entity Framework Core** - ORM
- **SQL Server** - Banco de dados
- **BCrypt.Net** - Criptografia de senhas
- **JWT Bearer** - Autenticação
- **Swagger** - Documentação da API

### Frontend
- **Blazor Server** - Painel Administrativo
- **Blazor Web** - Landing Page pública
- **Bootstrap 5** - Framework CSS

### Integrações
- **ASAAS** - Gateway de pagamento (PIX, Boleto, Cartão)
- **SMTP** - Envio de emails

## 📁 Estrutura do Projeto

```
CNHVirtual/
├── CNHVirtualAPI/          # API Backend
│   ├── Controllers/        # Endpoints da API
│   ├── Services/          # Lógica de negócio
│   ├── Models/            # Modelos de dados
│   ├── Data/              # Contexto do EF Core
│   └── DTOs/              # Data Transfer Objects
├── CNHVirtualADM/         # Painel Administrativo (Blazor Server)
│   ├── Components/        # Componentes Blazor
│   └── Pages/             # Páginas administrativas
├── CNHVirtual/            # Landing Page (Blazor Web)
│   ├── Components/        # Componentes Blazor
│   └── Pages/             # Páginas públicas
└── database/              # Scripts SQL
    ├── 00_setup_completo.sql
    ├── 01_create_database.sql
    ├── 02_create_tables.sql
    ├── 03_seed_data.sql
    └── 04_migrations_fix.sql
```

## 🔧 Configuração e Instalação

### Pré-requisitos
- .NET SDK 10.0
- SQL Server 2019+
- Visual Studio 2022 ou VS Code

### 1. Configurar Banco de Dados

Execute os scripts SQL na ordem:

```bash
sqlcmd -S SEU_SERVIDOR -E -i database/00_setup_completo.sql
```

Ou execute individualmente:
1. `01_create_database.sql` - Cria o banco PNHDigitalDB
2. `02_create_tables.sql` - Cria todas as tabelas
3. `03_seed_data.sql` - Dados iniciais
4. `04_migrations_fix.sql` - Migrações e correções

### 2. Configurar API

Edite `CNHVirtualAPI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR;Database=PNHDigitalDB;Integrated Security=True;TrustServerCertificate=True;"
  },
  "Asaas": {
    "ApiKey": "SUA_CHAVE_API_ASAAS",
    "Environment": "production",
    "ApiUrl": "https://www.asaas.com/api/v3"
  },
  "Jwt": {
    "Secret": "sua_chave_jwt_super_secreta",
    "Issuer": "CNHVirtualAPI",
    "Audience": "CNHVirtualApp",
    "ExpirationHours": 24
  }
}
```

### 3. Executar os Projetos

```bash
# Terminal 1 - API
cd CNHVirtualAPI
dotnet run

# Terminal 2 - Painel Admin
cd CNHVirtualADM
dotnet run

# Terminal 3 - Landing Page
cd CNHVirtual
dotnet run
```

## 🌐 URLs de Acesso

- **Landing Page**: http://localhost:5100
- **Painel Admin**: http://localhost:5296
- **API**: http://localhost:5273
- **Swagger**: https://localhost:7000

## 👤 Credenciais Padrão

### Painel Administrativo
- **Email**: admin@cnhvirtual.com.br
- **Senha**: Admin@2024

## 📋 Funcionalidades

### Landing Page
- ✅ Apresentação de planos
- ✅ Formulário de checkout
- ✅ Pagamento via PIX, Boleto ou Cartão
- ✅ Páginas de confirmação de pagamento

### Painel Administrativo
- ✅ Dashboard com estatísticas
- ✅ Gestão de planos e recursos
- ✅ Visualização de clientes e pedidos
- ✅ Configurações de email e pagamento
- ✅ Logs de webhooks

### API
- ✅ Autenticação JWT
- ✅ CRUD de planos
- ✅ Processamento de pagamentos
- ✅ Webhooks do ASAAS
- ✅ Envio de emails automáticos
- ✅ Modo de simulação (sem chave API)

## 🔐 Segurança

- Senhas criptografadas com BCrypt
- Autenticação JWT
- Validação de dados no servidor
- CORS configurado
- SQL Injection prevention (EF Core)

## 🧪 Modo de Simulação

O sistema possui um modo de simulação que permite testar pagamentos sem integração real com o ASAAS:

1. Deixe o campo `ApiKey` vazio em `appsettings.json`
2. O sistema automaticamente gerará pagamentos simulados
3. Útil para desenvolvimento e testes

## 📊 Banco de Dados

### Principais Tabelas
- **AdminUsers** - Usuários administrativos
- **Clientes** - Clientes cadastrados
- **Planos** - Planos de curso
- **Pedidos** - Pedidos realizados
- **Pagamentos** - Pagamentos processados
- **Assinaturas** - Assinaturas ativas
- **Configuracoes** - Configurações do sistema
- **EmailTemplates** - Templates de email

## 🔄 Integração ASAAS

O sistema suporta três formas de pagamento via ASAAS:

1. **PIX** - Pagamento instantâneo com QR Code
2. **Boleto Bancário** - Compensação em 1-3 dias úteis
3. **Cartão de Crédito** - Aprovação imediata

### Configurar ASAAS

1. Crie uma conta em https://www.asaas.com
2. Obtenha sua chave API em Configurações → Integrações
3. Configure a chave em `appsettings.json`
4. Configure webhooks apontando para: `https://seu-dominio.com/api/webhook/asaas`

## 📧 Configuração de Email

Para habilitar o envio de emails:

1. Acesse o Painel Administrativo
2. Vá em Configurações → Email
3. Configure servidor SMTP, porta, usuário e senha
4. Os templates de email podem ser personalizados

## 🐛 Troubleshooting

### Erro: "Cannot insert NULL into column"
Execute o script de migrações: `database/04_migrations_fix.sql`

### API não conecta ao banco
Verifique a connection string em `appsettings.json`

### Webhooks não funcionam
Verifique se a URL está acessível publicamente e configurada no ASAAS

### Botões de pagamento não respondem
Certifique-se de que o componente tem `@rendermode="InteractiveServer"`

## 📝 Licença

Este projeto é proprietário. Todos os direitos reservados.

## 👨‍💻 Desenvolvedor

Desenvolvido por Ediel Senn

---

**Versão**: 1.0.0
**Última atualização**: Fevereiro 2026
