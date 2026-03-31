# Sistema de Agendamentos de Salas Coworking

Aplicacao desktop em Windows Forms (.NET 8) com PostgreSQL para cadastro de salas e agendamentos.

## Aderencia ao teste

O repositorio atende ao enunciado do PDF com:

- cadastro de salas
- cadastro de agendamentos
- banco com tabelas `sala`, `agendamento` e `log_operacao`
- triggers para registrar `INSERT`, `UPDATE` e `DELETE`
- validacoes obrigatorias executadas no banco de dados

Validacoes implementadas no banco:

- todos os campos obrigatorios
- nome da sala unico
- data/hora final maior que a inicial
- bloqueio de sobreposicao de agendamentos para a mesma sala
- bloqueio de exclusao de sala com agendamento futuro

## Tecnologias

- .NET 8
- Windows Forms
- PostgreSQL
- Npgsql

## Estrutura

```text
.
|-- CoworkingAgendamento.csproj
|-- Teste 1.sln
|-- README.md
|-- NuGet.Config
|-- script.sql
|-- Data/
|   `-- Database.cs
|-- Forms/
|   |-- AgendamentoForm.cs
|   |-- MainForm.cs
|   |-- SalaForm.cs
|   `-- UiHelpers.cs
|-- Models/
|   |-- Agendamento.cs
|   `-- Sala.cs
`-- Repositories/
    |-- AgendamentoRepository.cs
    `-- SalaRepository.cs
```

## Como executar

1. Crie o banco:

```sql
CREATE DATABASE coworking;
```

2. Execute o script SQL:

```powershell
psql -U postgres -d coworking -f .\\script.sql
```

3. Configure a conexao com o PostgreSQL usando uma destas opcoes:

Opcao A, via variavel de ambiente:

```powershell
$env:COWORKING_CONNECTION_STRING = "Host=localhost;Port=5432;Database=coworking;Username=postgres;Password=SUA_SENHA"
```

Opcao B, via arquivo local `connection-string.txt` na raiz do projeto ou ao lado do executavel:

```text
Host=localhost;Port=5432;Database=coworking;Username=postgres;Password=SUA_SENHA
```

4. Execute a aplicacao:

```powershell
dotnet run --project .\\CoworkingAgendamento.csproj
```

## Observacoes

- O repositorio foi limpo para GitHub e não tem `bin/`, `obj/`, `.dotnet/` ou `.nuget/`.
- O arquivo obrigatorio do teste para criacao do banco esta em `script.sql`.
- A aplicacao aceita a variavel `COWORKING_CONNECTION_STRING` ou um arquivo local `connection-string.txt`; nao ha senha hardcoded no codigo.
