# Operations runbook

## Start locally

For a UI-only review, run the API and frontend in Demo mode as described in the README. For the production-shaped Compose profile, create `.env` from `.env.example` first.

Verify:

```powershell
Invoke-WebRequest http://localhost:5000/health/live
Invoke-WebRequest http://localhost:5000/health/ready
```

## Connect to a cluster

Set `Kubernetes__Mode=Cluster` and provide kubeconfig or in-cluster service-account credentials. Open Settings and run the connection check. A `SetupRequired` response means the API is running but cannot authenticate to the Kubernetes API.

Check the identity before changing resources:

```powershell
kubectl auth can-i get nodes
kubectl auth can-i create deployments --all-namespaces
kubectl auth can-i get pods --all-namespaces
```

## Common incidents

| Symptom | First checks | Response |
|---|---|---|
| Cluster shows SetupRequired | kubeconfig path, context, service-account token, API reachability | Fix credentials or context, then retry Settings |
| CPU/memory shows `—` | `kubectl get apiservice v1beta1.metrics.k8s.io` | Install or repair metrics-server; telemetry is intentionally not fabricated |
| Deployment apply returns AlreadyExists | API logs and resource status | Re-run the operation; reconciliation now reads and replaces existing resources |
| API readiness fails | PostgreSQL connection, JWT key, Kubernetes mode | Check `/health/ready`, database connectivity, and configuration values |
| Secret or JWT rotation is required | Deployment secret references and Terraform variables | Rotate in the external secret source, restart workloads, and verify login/health |

## Rollback

Use the image tag from the last known-good release and roll back the Helm release. Do not use `latest` for production promotion; publish immutable tags or digests from CI.

## Evidence to capture

For an incident or customer handoff, record the cluster context, deployment/image digest, relevant Kubernetes events, API logs, health results, and the exact change or rollback command.
