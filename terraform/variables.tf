variable "environment" {
  type = string
  default = "dev"
}
variable "kubeconfig_path" {
  type = string
  default = "~/.kube/config"
}
variable "namespace" {
  type = string
  default = "vertex"
}
variable "api_image" {
  type = string
  default = "ghcr.io/vertex/vertex-api:latest"
}
variable "frontend_image" {
  type = string
  default = "ghcr.io/vertex/vertex-frontend:latest"
}
variable "domain" {
  type = string
  default = "vertex.local"
}
variable "jwt_key" {
  type = string
  sensitive = true
  default = "change-me-in-production-vertex-jwt-key"
}
