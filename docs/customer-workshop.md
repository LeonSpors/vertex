# Customer workshop outline

## 90-minute agenda

1. Goals, teams, and current delivery pain points — 10 minutes
2. Application onboarding and environment lifecycle — 15 minutes
3. Cluster, network, identity, and compliance constraints — 20 minutes
4. Platform operating model and ownership boundaries — 15 minutes
5. Architecture options and decision matrix — 20 minutes
6. Acceptance criteria, risks, and next steps — 10 minutes

## Discovery questions

- Which teams deploy, approve, and operate workloads?
- Which environments are permanent, preview, or ephemeral?
- Is the target platform cloud, on-premises, or multi-cloud?
- Which identity provider, secret manager, registry, and Git hosting system are mandatory?
- What are the recovery-time, availability, data-residency, and audit requirements?
- Which Kubernetes resources may the platform create, update, or delete?

## Decision matrix

| Area | Options to compare | Decision criteria |
|---|---|---|
| CNI | Cilium, Calico, cloud-native CNI | Network policy, observability, performance, operations |
| Ingress | NGINX, cloud load balancer, Gateway API | TLS lifecycle, routing, WAF, ownership |
| Secrets | External Secrets, Vault, cloud secret manager | Rotation, auditability, workload identity |
| Delivery | Argo CD, Flux, controlled CI deploy | Drift detection, promotion, rollback, team skills |
| Policy | Pod Security Standards, Kyverno, OPA Gatekeeper | Guardrails, exceptions, reporting, compliance |

## Workshop deliverables

- Current-state and target-state architecture
- Responsibility matrix for platform and application teams
- Selected-tool comparison with rejected alternatives
- Security and compliance control map
- Thin-slice implementation plan with measurable acceptance criteria
