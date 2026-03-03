````md
# 🏗 CivilWorks

Sistema de **Gestão de Obras para Engenharia Civil**, desenvolvido em **ASP.NET Core 8** com arquitetura em camadas e preparado para ambiente **SaaS (Multiempresa)**.

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
````

---

## 📌 Funcionalidades Atuais

### 🏗 Obras

* Cadastro
* Edição
* Exclusão lógica
* Status
* Progresso (%)
* Orçamento previsto

### 💰 Financeiro por Obra

* Lançamentos (Receita / Despesa)
* Controle de saldo
* Alerta de estouro de orçamento
* Despesas por categoria

### 👥 Usuários

* Perfis: **Admin**, **Engenheiro**, **Funcionário**
* Permissões por perfil
* Usuários vinculados à empresa
* Autenticação com ASP.NET Identity

---

## 🏢 Multiempresa (SaaS Ready)

Cada usuário pertence a uma empresa.
Todos os dados são filtrados por `EmpresaId`.

Estrutura preparada para se tornar um SaaS.

---

## 🔐 Matriz de Permissões

| Perfil      | Criar | Editar | Excluir |
| ----------- | :---: | :----: | :-----: |
| Admin       |   ✔   |    ✔   |    ✔    |
| Engenheiro  |   ✔   |    ✔   |    ✖    |
| Funcionário |   ✖   |    ✖   |    ✖    |

---

## ⚙️ Como rodar localmente

### 1) Configure a Connection String

No arquivo `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CivilWorks;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 2) Aplicar migrations

```bash
dotnet ef database update
```

### 3) Executar o projeto

```bash
dotnet run --project src/CivilWorks.Web
```

---

## 📈 Próximos Passos (Roadmap)

* Dashboard financeiro geral
* Relatórios exportáveis (PDF / Excel)
* Controle de estoque
* Gestão de funcionários por obra
* Deploy em nuvem (Azure)

---

## 👨‍💻 Autor

Desenvolvido por **Gabriel Neves**.

```