# .NET 9 Production-Ready Microservice Template

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL 17](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20%2B%20DDD%20%2B%20Vertical%20Slices-success)](#arquitetura-e-camadas)

Template arquitetural opinativo e pronto para produção voltado à construção de microsserviços distribuídos em **.NET 9**. Este repositório representa a **Fase 1** de uma plataforma de e-commerce distribuída, fornecendo o alicerce padronizado (observabilidade, persistência, resiliência, segurança e testes automatizados) para os serviços de domínio das Fases 2 e 3.

---

## Sumário

- [Visão Arquitetural e Camadas](#visão-arquitetural-e-camadas)
- [Decisões Arquiteturais (ADRs)](#decisões-arquiteturais-adrs)
- [Cuidados de Segurança e Performance](#cuidados-de-segurança-e-performance)
- [Stack Tecnológica e Bibliotecas](#stack-tecnológica-e-bibliotecas)
- [Como Executar Localmente](#como-executar-localmente)
- [Limitações Atuais e Roadmap (Fases 2 e 3)](#limitações-atuais-e-roadmap-fases-2-e-3)
- [Uso de Inteligência Artificial no Desenvolvimento](#uso-de-inteligência-artificial-no-desenvolvimento)

---

## Visão Arquitetural e Camadas

A solução adota **Clean Architecture** com inversão de dependência estrita, combinada com **DDD Tático** no núcleo e **Vertical Slices** na camada de casos de uso:

```text
dotnet-microservice-template/
├── Directory.Build.props            # Configurações globais de compilação e Analyzers (.NET 9)
├── Directory.Packages.props         # Central Package Management (CPM)
├── docker-compose.yml               # Infraestrutura local (PostgreSQL 17)
├── src/
│   ├── BuildingBlocks.Core/         # Primitivas puras: Entity, AggregateRoot, ValueObject, IDomainEvent, Result<T>
│   ├── Service.Domain/              # Entidades ricas, invariantes de negócio, Domain Events e erros tipados
│   ├── Service.Application/         # Casos de uso (Vertical Slices), Dispatcher CQRS próprio e Pipeline Behaviors
│   ├── Service.Infrastructure/      # EF Core 9 + PostgreSQL, SaveChangesInterceptors (Auditoria/Outbox) e JWT
│   └── Service.API/                 # Minimal APIs auto-descobertas, RFC 9457 ProblemDetails, Scalar e HealthChecks
└── tests/
    ├── Service.UnitTests/           # Testes de domínio e invariantes (xUnit + FluentAssertions)
    └── Service.IntegrationTests/    # Testes end-to-end com PostgreSQL real em container (Testcontainers)
```

# Decisões Arquiteturais (ADRs)

## 1\. Decisões Arquiteturais

### CQRS Nativo (Zero MediatR)

Em vez de acoplar a camada de Application a bibliotecas externas de mediação, foi implementado um `IDispatcher` leve e fortemente tipado sobre o container nativo de DI do .NET, suportando `IPipelineBehavior<TRequest, TResponse>` (`LoggingBehavior` e `ValidationBehavior`).

### Result Pattern vs. Exceptions

Falhas esperadas de domínio e validação não lançam exceções, evitando o overhead de _stack unwinding_.

Toda operação retorna `Result` ou `Result<TValue>`, traduzidos automaticamente na borda HTTP para o padrão RFC 9457 (`ProblemDetails`).

Exceções ficam restritas a falhas catastróficas, capturadas pelo `IExceptionHandler` global.

### Vertical Slices na Camada de Aplicação

Funcionalidades são agrupadas por caso de uso:

text

```text
Features/
└── Items/
    ├── CreateItem/
    └── GetItemById/
```

Em vez de pastas técnicas fragmentadas, essa abordagem aumenta a coesão e facilita a manutenção.

### EF Core Interceptors para Auditoria e Outbox

O preenchimento de timestamps (`AuditableEntityInterceptor`, com `TimeProvider` testável) e a captura de eventos de domínio (`OutboxMessagesInterceptor`) ocorrem atomicamente durante o `SaveChangesAsync`, preparando o terreno para o **Transactional Outbox Pattern**.

### UUID v7 (`Guid.CreateVersion7()`)

Entidades utilizam UUIDs ordenáveis por timestamp nativos do .NET 9, reduzindo a fragmentação de índices B-Tree no PostgreSQL.

### Minimal APIs + OpenAPI + Scalar

Substituição de Controllers pesados e do descontinuado Swashbuckle por endpoints modulares (`IEndpoint`), auto-registrados, com documentação interativa via Scalar.

---

## 2\. Cuidados de Segurança e Performance

### Fail-Fast na Validação de Segredos JWT

A aplicação recusa iniciar caso `Jwt:SecretKey` esteja ausente ou possua menos de 32 bytes (256 bits), impedindo fallbacks inseguros em produção.

### Autenticação Stateless entre Serviços

Validação de tokens JWT com `ClockSkew = TimeSpan.Zero` e desacoplamento do `HttpContext` via `IUserContext`.

### Prevenção de Vazamento de Stack Trace

O `GlobalExceptionHandler` diferencia erros de sintaxe de payload (`BadHttpRequestException` → `400 Bad Request`) de falhas internas (`500 Internal Server Error`), expondo apenas um `traceId` para correlação de logs.

### Hot-Paths Zero-Allocation & Compiled Expressions

- Logs estruturados nos behaviors e handlers utilizam `[LoggerMessage]` (Source Generators), evitando boxing e alocações em memória.
- A construção de falhas de validação no `ValidationBehavior` utiliza `Expression.Lambda` compilada e cacheada em `ConcurrentDictionary`, eliminando `MethodInfo.Invoke` via reflection em runtime.

### Health Checks Granulares

Separação explícita entre:

- `/health/live`: Liveness.
- `/health/ready`: Readiness, com verificação ativa no PostgreSQL.

Essa separação atende às necessidades de orquestradores como Kubernetes e ECS.

---

## 3\. Stack Tecnológica e Bibliotecas

| Categoria            | Tecnologia / Pacote                                             | Finalidade                                                           |
| -------------------- | --------------------------------------------------------------- | -------------------------------------------------------------------- |
| Runtime & Linguagem  | .NET 9 / C# 13                                                  | Plataforma principal com analyzers estritos ativados                 |
| Banco de Dados & ORM | PostgreSQL 17, EF Core 9, Npgsql, EFCore.NamingConventions      | Persistência relacional com convenção snake_case automática          |
| Validação            | FluentValidation                                                | Regras de entrada desacopladas executadas no pipeline CQRS           |
| Resiliência HTTP     | Microsoft.Extensions.Http.Resilience (Polly v8)                 | Políticas de retry, timeout e circuit breaker para chamadas externas |
| Documentação & Logs  | Microsoft.AspNetCore.OpenApi, Scalar.AspNetCore, Serilog        | Contrato OpenAPI 3.1, UI moderna e logging estruturado               |
| Testes               | xUnit, FluentAssertions, NSubstitute, Testcontainers.PostgreSql | Testes unitários rápidos e testes de integração isolados com Docker  |

---

## 4\. Como Executar Localmente

### Pré-requisitos

- .NET 9 SDK
- Docker
- Docker Compose

### Passo a Passo

**1\. Suba o banco de dados PostgreSQL:**

bash

```bash
docker compose up -d
```

**2\. Execute a API:**

As migrations são aplicadas automaticamente em ambiente `Development`.

bash

```bash
dotnet run --project src/Service.API
```

**3\. Acesse a documentação interativa (Scalar):**

text

```text
http://localhost:5000/scalar/v1
```

> Verifique a porta exibida no console, pois ela pode variar conforme a configuração local.

**4\. Verifique os Health Checks:**

- Liveness: `/health/live`
- Readiness: `/health/ready`

**5\. Execute a suíte completa de testes:**

Inclui testes unitários e testes de integração com Testcontainers.

bash

```bash
dotnet test
```

---

## 5\. Limitações Atuais e Roadmap (Fases 2 e 3)

Como este repositório é o template base (Fase 1), algumas capacidades distribuídas foram intencionalmente preparadas em estrutura, mas ainda não possuem processamento ativo.

### Processamento Assíncrono do Outbox (Fase 3)

A tabela `outbox_messages` (com coluna `jsonb` e índice filtrado) e o `OutboxMessagesInterceptor` já persistem eventos de domínio na mesma transação do banco.

Porém, o Background Worker (ou CDC) para publicação no message broker (RabbitMQ/Kafka) será implementado na Fase 3.

### Distributed Tracing & Métricas (Fase 3)

O projeto conta com `traceId` e Serilog estruturado, mas a instrumentação completa com OpenTelemetry será implementada na Fase 3, incluindo:

- Traces distribuídos W3C entre microsserviços.
- Exportação para Jaeger.
- Integração com Prometheus.

### Rate Limiting & Idempotência de Webhooks (Fase 2)

Esses recursos serão especializados nos microsserviços concretos.

Exemplo: serviço de Pagamentos com integração Mercado Pago.

---

## 6\. Uso de Inteligência Artificial no Desenvolvimento

Este projeto foi desenvolvido com apoio de IA generativa atuando como par de revisão arquitetural (_Staff Engineer sparring partner_), seguindo diretrizes rigorosas de engenharia.

### Revisão Crítica de Legado

Diagnóstico de débitos técnicos de uma base anterior:

- Acoplamento de handlers no domínio.
- Repositórios genéricos desnecessários.
- Fragmentação de projetos.

### Refinamento de Design e Segurança

Validação de trade-offs arquiteturais e decisões técnicas:

- CQRS próprio vs. bibliotecas de terceiros.
- Result Pattern com RFC 9457.
- Prevenção de fallback inseguro em chaves JWT.
- Eliminação de reflection em hot-paths.

### Validação Determinística

Todo código sugerido foi submetido aos analisadores estáticos do .NET 9 (`AnalysisLevel: latest-recommended`, com 0 warnings), compilado e validado por testes automatizados contra instâncias reais do PostgreSQL via Testcontainers.
