module "network" {
  source      = "./modules/network"
  namespace   = var.namespace
  environment = var.environment
}

module "monitoring" {
  source    = "./modules/monitoring"
  namespace = var.namespace
  enabled   = true
}

module "opsdeck" {
  source            = "./modules/opsdeck"
  namespace         = var.namespace
  chart_path        = "${path.root}/../helm/vertex"
  api_image         = var.api_image
  frontend_image    = var.frontend_image
  domain            = var.domain
  jwt_key           = var.jwt_key
  postgres_password = var.postgres_password
  depends_on        = [module.network]
}
