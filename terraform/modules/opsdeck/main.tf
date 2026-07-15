resource "kubernetes_secret" "postgres" {
  metadata { name = "vertex-postgresql", namespace = var.namespace }
  data = { "connection-string" = "Host=vertex-postgresql;Port=5432;Database=vertex;Username=vertex;Password=change-me" }
  type = "Opaque"
}

resource "helm_release" "postgres" {
  name = "vertex-postgresql"
  namespace = var.namespace
  repository = "https://charts.bitnami.com/bitnami"
  chart = "postgresql"
  version = "16.4.5"
  set { name = "auth.database", value = "vertex" }
  set { name = "auth.username", value = "vertex" }
  set { name = "auth.password", value = "change-me" }
  set { name = "primary.persistence.size", value = "20Gi" }
}

resource "helm_release" "redis" {
  name = "vertex-redis"
  namespace = var.namespace
  repository = "https://charts.bitnami.com/bitnami"
  chart = "redis"
  version = "20.6.2"
  set { name = "architecture", value = "standalone" }
  set { name = "master.persistence.size", value = "8Gi" }
}

resource "helm_release" "vertex" {
  name = "vertex"
  namespace = var.namespace
  chart = var.chart_path
  values = [yamlencode({
    api = {
      image = { repository = split(":", var.api_image)[0], tag = split(":", var.api_image)[1] }
    }
    frontend = {
      image = { repository = split(":", var.frontend_image)[0], tag = split(":", var.frontend_image)[1] }
    }
    ingress = { host = var.domain, tls = false }
    postgres = { existingSecret = kubernetes_secret.postgres.metadata[0].name }
  })]
  depends_on = [helm_release.postgres, helm_release.redis]
}
