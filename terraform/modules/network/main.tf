resource "kubernetes_namespace" "vertex" {
  metadata {
    name = var.namespace
    labels = {
      "app.kubernetes.io/part-of"           = "vertex"
      "vertex.dev/environment"              = var.environment
      "pod-security.kubernetes.io/enforce" = "baseline"
      "pod-security.kubernetes.io/audit"   = "restricted"
      "pod-security.kubernetes.io/warn"    = "restricted"
    }
  }
}

resource "kubernetes_resource_quota" "platform" {
  metadata { name = "vertex-quota", namespace = kubernetes_namespace.vertex.metadata[0].name }
  spec {
    hard = { pods = "100", "requests.cpu" = "16", "requests.memory" = "32Gi", "limits.cpu" = "32", "limits.memory" = "64Gi" }
  }
}

resource "kubernetes_limit_range" "defaults" {
  metadata { name = "vertex-defaults", namespace = kubernetes_namespace.vertex.metadata[0].name }
  spec {
    limit { type = "Container", default = { cpu = "500m", memory = "512Mi" }, default_request = { cpu = "100m", memory = "128Mi" } }
  }
}
