# Security

Vertex is an architectural MVP and a demonstration of a Kubernetes control plane. The repository is safe to run locally in Demo mode, but a real deployment still requires environment-specific identity, secret management, and review.

## Reporting a vulnerability

Please do not publish exploit details in a public issue. Use a private GitHub security advisory for the repository. Include the affected component, reproduction steps, impact, and a suggested mitigation where possible.

## Security controls

- API endpoints require a bearer token except for login and health checks.
- Demo credentials and sample data are seeded only when `Kubernetes:Mode=Demo`.
- Compose and Terraform require deployment credentials through environment-backed variables.
- Kubernetes workloads use non-root execution, RuntimeDefault seccomp, dropped capabilities, probes, resource limits, and network policies.
- CI runs secret scanning, .NET and npm vulnerability checks, CodeQL, Terraform validation, Helm rendering, container scanning, and SBOM generation.
- The Kubernetes service account rules are explicit rather than using wildcard resources or verbs.

## Threat-model boundaries

Vertex protects the control-plane API and its Kubernetes connection. It does not replace a cluster’s identity provider, admission policy, cloud secret manager, image registry policy, or organization-wide audit platform.

Before production use, replace the demo JWT login with OIDC, move secret values to a dedicated secret manager, add application-level roles and audit events, and scope Kubernetes write permissions to the namespaces and resources owned by each tenant.

## Local safety

Use `.env.example` only as a template. Never commit `.env`, Terraform state, kubeconfig files, registry credentials, or real application secrets. Keep Demo mode isolated from production clusters.
