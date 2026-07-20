# Architecture decisions

## Scope

Vertex is a small internal developer platform. The control plane provides a browser console, a REST API, persistence for platform metadata, and adapters for Kubernetes operations. Demo mode makes the product reviewable without requiring a cluster; Cluster mode uses the official Kubernetes client and reports missing access through the Settings flow.

## Boundaries

```text
Browser -> API/Auth -> Application services -> Infrastructure ports
                                      |-> PostgreSQL metadata
                                      |-> Kubernetes API
                                      |-> Metrics API
```

The application layer owns use cases and validation. Infrastructure owns EF Core, JWT, Kubernetes connectivity, and provider-specific behavior. This keeps the UI and use cases testable while allowing the demo adapter to be replaced by cluster operations.

## Key decisions

| Decision | Rationale | Trade-off |
|---|---|---|
| Helm plus Terraform | Helm packages the platform; Terraform composes the cluster dependencies and release | Two tools require clear ownership and CI validation |
| Demo and Cluster adapters | Reviewers can run the UI without credentials while operators can connect to a real cluster | Demo behavior must never be mistaken for live telemetry |
| PostgreSQL for control-plane metadata | Durable state and relational constraints fit environments, deployments, and secrets metadata | Secret values need a production secret-manager integration |
| Kubernetes API as the data plane | The platform remains close to native Kubernetes resources and events | RBAC must be scoped carefully as tenant isolation grows |
| Prometheus and metrics-server integrations | Standard Kubernetes observability components reduce custom telemetry | Metrics are unavailable until the cluster dependencies are installed |

## Review questions

For a production design, validate the target cloud/on-premises provider, tenant boundary, identity provider, secret manager, GitOps controller, backup strategy, and compliance requirements before selecting CNI, ingress, storage, and policy components.
