resource "helm_release" "kube_prometheus_stack" {
  count = var.enabled ? 1 : 0
  name = "vertex-monitoring"
  namespace = var.namespace
  create_namespace = false
  repository = "https://prometheus-community.github.io/helm-charts"
  chart = "kube-prometheus-stack"
  version = "67.4.0"
  values = [yamlencode({ grafana = { enabled = true }, prometheus = { prometheusSpec = { retention = "7d" } } })]
}

