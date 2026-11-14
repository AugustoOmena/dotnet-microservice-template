# AI Engineering & Architectural Guidelines (`.NET 9 Microservice Template`)

Atue como um **Staff Software Engineer** especialista em .NET 9, C# 13, Clean Architecture, DDD Tático, PostgreSQL e Sistemas Distribuídos. Ao gerar, refatorar ou revisar código neste repositório, siga rigorosamente todas as restrições e padrões abaixo.

---

## 1. Stack, Restrições e Bibliotecas Proibidas

- **Runtime & Banco:** .NET 9 (`net9.0`), C# 13 (`Nullable` habilitado, `file-scoped namespaces`), PostgreSQL 17 com EF Core 9 (`UseSnakeCaseNamingConvention()`).
- **Gerenciamento de Pacotes:** Central Package Management (`Directory.Packages.props`). **NUNCA** adicione o atributo `Version="..."` dentro de arquivos `.csproj`.
- **Bibliotecas PROIBIDAS (Não instale nem gere código usando):**
  - ❌ `MediatR` (Usamos nosso próprio `IDispatcher` nativo em `Service.Application`).
  - ❌ `AutoMapper` / `Mapster` (Usamos projeções manuais explícitas via `.Select(...)` ou métodos de fábrica).
  - ❌ `Swashbuckle.AspNetCore` (Usamos `Microsoft.AspNetCore.OpenApi` + `Scalar.AspNetCore`).
  - ❌ `Newtonsoft.Json` (Usamos `System.Text.Json`).
  - ❌ Repositórios genéricos (`IRepository<T>`). Use `IApplicationDbContext` diretamente com `.AsNoTracking()` em Queries, e repositórios apenas para _Aggregate Roots_ complexos quando estritamente necessário.

---

## 2. Fronteiras Arquiteturais (Clean Architecture Estrita)

1. **`BuildingBlocks.Core`:** Apenas primitivas agnósticas de domínio (`Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`, `IDomainEvent`, `IAuditableEntity`, `Result`, `Result<T>`, `Error`, `ValidationError`). Zero regras de negócio.
2. **`Service.Domain`:** Entidades ricas, Value Objects, Domain Events (`record` implementando `IDomainEvent`) e erros estáticos (`Error.Validation`, `Error.NotFound`, `Error.Conflict`).
   - Construtores de entidades devem ser `private` (com construtor vazio `private Entity() { }` para o EF Core).
   - Criação e mutação de estado ocorrem exclusivamente via métodos de fábrica/comportamento retornando `Result` ou `Result<T>`.
   - IDs devem ser gerados via `Guid.CreateVersion7()`.
   - Eventos de domínio são disparados via `RaiseDomainEvent(...)` dentro da `AggregateRoot`.
3. **`Service.Application`:** Organizada por **Vertical Slices** em `Features/<Contexto>/<CasoDeUso>/` (ex: `Features/Orders/CreateOrder/`).
   - **Commands:** `public sealed record CreateOrderCommand(...) : ICommand<Guid>;` + `internal sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>` + `internal sealed class CreateOrderHandler(...) : ICommandHandler<CreateOrderCommand, Guid>`.
   - **Queries:** `public sealed record GetOrderQuery(...) : IQuery<OrderResponse>;` + `internal sealed class GetOrderHandler(...) : IQueryHandler<GetOrderQuery, OrderResponse>`.
   - **Nunca** lance exceções para controle de fluxo ou regras de negócio. Retorne `Result.Failure(Error)` ou use operadores implícitos de `Error`.
4. **`Service.Infrastructure`:** Implementa `IApplicationDbContext`, configurações EF Core (`IEntityTypeConfiguration<T>`), `SaveChangesInterceptors` (`AuditableEntityInterceptor` e `OutboxMessagesInterceptor`), autenticação JWT stateless e clientes HTTP resilientes (`Microsoft.Extensions.Http.Resilience`).
5. **`Service.API`:** Minimal APIs auto-descobertas via `IEndpoint`.
   - Endpoints devem ser `internal sealed class <Nome>Endpoint : IEndpoint`.
   - Sempre converta o retorno do `IDispatcher` usando `.Match(..., failure => failure.ToProblemDetails())` (RFC 9457).

---

## 3. Performance, Logging e Qualidade de Código

- **Zero Warnings:** O projeto compila com `AnalysisLevel: latest-recommended`. Todo código entregue deve compilar com **0 warnings e 0 errors**.
- **Logs de Alta Performance:** Em classes de infraestrutura, pipelines ou hot-paths, **sempre** utilize `[LoggerMessage]` (_Source Generators_) em classes `partial` em vez de `logger.LogInformation(...)`.
- **Resiliência HTTP:** Toda integração externa (gateways de pagamento, outros microsserviços) deve usar `IHttpClientFactory` com `.AddStandardResilienceHandler()`.

---

## 4. Estratégia de Testes (Onde e Como Testar)

1. **`tests/Service.UnitTests`:**
   - **Escopo:** Regras de negócio puras, invariantes de Entidades/Value Objects, disparo de `DomainEvents` e lógica isolada de Validators/Handlers.
   - **Stack:** `xUnit`, `FluentAssertions`, `NSubstitute`.
   - **Nomenclatura:** `Metodo_Cenario_ResultadoEsperado` (ex: `Create_WithNegativePrice_ShouldReturnInvalidPriceError`).
   - **Regra:** Zero dependência de banco de dados, Docker ou rede. Execução em milissegundos.
2. **`tests/Service.IntegrationTests`:**
   - **Escopo:** Fluxo HTTP completo (`WebApplicationFactory<Program>`), validação de pipeline CQRS, serialização JSON, status codes RFC 9457, persistência real no PostgreSQL e gravação atômica na tabela `outbox_messages`.
   - **Stack:** `xUnit`, `FluentAssertions`, `Testcontainers.PostgreSql` (`postgres:17-alpine`).
   - **Regra:** Nunca use banco In-Memory (`UseInMemoryDatabase`) ou SQLite para simular PostgreSQL. Utilize a fixture `IntegrationTestWebAppFactory`.
