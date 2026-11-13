# .NET 9 Production-Ready Microservice Template

Template arquitetural moderno e opinativo para microsserviços em **.NET 9**, construído com **Clean Architecture**, **DDD Tático**, **CQRS Nativo (Zero MediatR)**, **Result Pattern** e **PostgreSQL**.

## Arquitetura & Camadas

- **`BuildingBlocks.Core`**: Primitivas reutilizáveis de domínio e controle de fluxo (`Entity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`, `Result<T>`, `Error`).
- **`Service.Domain`**: Núcleo de regras de negócio, invariantes, eventos de domínio e contratos de repositórios de agregados.
- **`Service.Application`**: Casos de uso organizados em _Vertical Slices_, dispatcher CQRS próprio, _Pipeline Behaviors_ (Validação, Logging, Transação) e abstrações.
- **`Service.Infrastructure`**: Persistência com EF Core 9 + PostgreSQL, _SaveChangesInterceptors_ (Auditoria & Transactional Outbox), autenticação JWT _stateless_ e clientes HTTP resilientes.
- **`Service.API`**: Minimal APIs auto-descobertas, tratamento global de erros padronizado via **RFC 9457 (`ProblemDetails`)**, OpenAPI + Scalar e Health Checks.
- **`tests/`**: Testes unitários (`Service.UnitTests`) e de integração com banco real via Testcontainers (`Service.IntegrationTests`).
