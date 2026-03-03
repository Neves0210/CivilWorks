# 🏗 CivilWorks

Sistema de **Gestão de Obras para Engenharia Civil**, desenvolvido em **ASP.NET Core** com arquitetura em camadas e preparado para ambiente **SaaS (Multiempresa)**.

> Plataforma para controle completo de obras, financeiro e usuários com segregação por empresa.

---

## 🚀 Tecnologias

- **ASP.NET Core 8**
- **Entity Framework Core**
- **SQL Server**
- **ASP.NET Identity**
- **Bootstrap 5**
- **Arquitetura em Camadas** (Domain / Application / Infrastructure / Web)

---

## ✅ Principais Funcionalidades

### 🏗 Obras
- Cadastro / edição / exclusão lógica
- Status da obra
- Progresso (%)
- Orçamento previsto
- Tela de detalhes com visão consolidada

### 💰 Financeiro por Obra
- Lançamentos (Receita / Despesa)
- Totais automáticos
- Saldo da obra
- Despesas por categoria
- Alerta de estouro de orçamento

### 🧾 Histórico (Auditoria)
- Registro automático de eventos
- Controle de alterações
- Log por usuário

### 🔐 Usuários e Permissões
- Perfis: **Admin**, **Engenheiro**, **Funcionário**
- Controle por Roles
- Usuários vinculados à empresa
- Estrutura preparada para SaaS

---

## 🏢 Multiempresa (SaaS Ready)

Cada usuário pertence a uma empresa.

Todos os dados são filtrados por:

```

EmpresaId

```

Isso garante isolamento total entre empresas.

---

## 🏛 Estrutura do Projeto

```

CivilWorks/
├── src/
│   ├── CivilWorks.Domain/         → Entidades e regras de negócio
│   ├── CivilWorks.Application/    → Casos de uso e serviços
│   ├── CivilWorks.Infrastructure/ → EF Core, Identity, Repositórios
│   └── CivilWorks.Web/            → Controllers, Views e Interface
├── .gitignore
├── README.md
└── CivilWorks.sln

```

---

## ⚙️ Como Rodar Localmente

### 1️⃣ Configurar Connection String (DEV)

Crie o arquivo:

```

src/CivilWorks.Web/appsettings.Development.json

````

Com o conteúdo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CivilWorks;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
````

---

### 2️⃣ Aplicar Migrations

```bash
dotnet ef database update --project src/CivilWorks.Infrastructure --startup-project src/CivilWorks.Web
```

---

### 3️⃣ Executar o Projeto

```bash
dotnet run --project src/CivilWorks.Web
```

---

## 🔐 Permissões (Resumo)

| Perfil      | Ver Obras | Criar/Editar | Excluir | Criar Lançamento | Excluir Lançamento |
| ----------- | --------: | -----------: | ------: | ---------------: | -----------------: |
| Admin       |         ✅ |            ✅ |       ✅ |                ✅ |                  ✅ |
| Engenheiro  |         ✅ |            ✅ |       ❌ |                ✅ |                  ✅ |
| Funcionário |         ✅ |            ❌ |       ❌ |                ✅ |                  ❌ |

---

## 🗺 Roadmap

### 🔹 v0.1.0 (Atual)

* Base SaaS com EmpresaId
* Obras + Financeiro
* Dashboard
* Permissões por perfil

### 🔹 v0.2.0

* Checklist por obra
* Cronograma de tarefas
* Relatórios PDF/Excel

### 🔹 v0.3.0

* Controle de estoque
* Centro de custos
* Permissões avançadas

### 🔹 v1.0.0

* Portal do cliente
* Admin master multiempresa
* Deploy em nuvem

---

## 📈 Objetivo

Transformar o CivilWorks em uma plataforma SaaS escalável para empresas de Engenharia Civil.

---

## 👨‍💻 Autor

**Gabriel Neves**

````
