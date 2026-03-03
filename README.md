# 🏗 CivilWorks

Sistema de **Gestão de Obras para Engenharia Civil**, desenvolvido em **ASP.NET Core** com arquitetura em camadas e preparado para ambiente **SaaS (Multiempresa)**.

> Plataforma para controle de obras, custos, lançamentos financeiros e usuários por empresa.

---

## ✅ Principais Funcionalidades

### 🏗 Obras
- Cadastro / edição / exclusão lógica
- Status e progresso (%)
- Orçamento previsto
- Tela de detalhes com visão financeira

### 💰 Financeiro por Obra
- Lançamentos (Receita / Despesa)
- Totais e saldo
- Despesas por categoria
- Alerta de estouro de orçamento

### 🧾 Histórico / Auditoria
- Registro de eventos (criação, edição, exclusão, lançamentos)

### 🔐 Usuários e Permissões
- Perfis: **Admin**, **Engenheiro**, **Funcionário**
- Controle por roles (backend protegido)
- Multiempresa por **EmpresaId**

---

## 🚀 Tecnologias

- **ASP.NET Core**
- **Entity Framework Core**
- **SQL Server**
- **ASP.NET Identity**
- **Bootstrap 5**
- **Arquitetura em Camadas** (Domain / Application / Infrastructure / Web)

---

## 🏛 Estrutura do Projeto

```text
CivilWorks/
├── src/
│   ├── CivilWorks.Domain/         → Entidades e regras de negócio
│   ├── CivilWorks.Application/    → Casos de uso e serviços
│   ├── CivilWorks.Infrastructure/ → EF Core, Identity, Repositórios
│   └── CivilWorks.Web/            → Controllers, Views, UI
├── .gitignore
├── README.md
└── CivilWorks.sln
⚙️ Como Rodar Localmente
1) Configurar connection string (DEV)

Crie o arquivo appsettings.Development.json em src/CivilWorks.Web/:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CivilWorks;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
2) Aplicar migrations
dotnet ef database update --project src/CivilWorks.Infrastructure --startup-project src/CivilWorks.Web
3) Rodar
dotnet run --project src/CivilWorks.Web
🔐 Permissões (Resumo)
Perfil	Ver Obras	Criar/Editar Obra	Excluir Obra	Criar Lançamento	Excluir Lançamento
Admin	✅	✅	✅	✅	✅
Engenheiro	✅	✅	❌	✅	✅
Funcionário	✅	❌	❌	✅	❌
🗺 Roadmap
v0.1.0 (atual)

Base SaaS (EmpresaId)

Obras + Lançamentos + Dashboard

Auditoria por obra

Permissões por perfil

v0.2.0

Checklist por obra

Cronograma (tarefas) por obra

Relatórios básicos (PDF/Excel)

v0.3.0

Estoque / materiais por obra

Centro de custos

Perfis refinados + permissões por feature

v1.0.0

Portal do cliente (visão limitada)

Multiempresa completo (admin master)

Deploy e monitoramento

👨‍💻 Autor

Gabriel Neves


> Depois a gente coloca prints e GIFs do sistema — isso dá muito “valor” no GitHub.

---

## 2) Badges (opcional mas recomendado)
Para badges de build, precisamos criar um workflow. Vamos fazer isso no passo 3 e depois adicionamos o badge. ✅

---

## 3) GitHub Actions (build automático)

Crie este arquivo:

`.github/workflows/build.yml`

Conteúdo:

```yaml
name: build

on:
  push:
    branches: [ "main" ]
  pull_request:
    branches: [ "main" ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "8.0.x"

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore -c Release