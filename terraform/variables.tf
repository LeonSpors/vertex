variable "environment" {
  type    = string
  default = "dev"
}
variable "kubeconfig_path" {
  type    = string
  default = "~/.kube/config"
}
variable "namespace" {
  type    = string
  default = "vertex"
}
variable "api_image" {
  type    = string
  default = "ghcr.io/vertex/vertex-api:v0.1.0"
}
variable "frontend_image" {
  type    = string
  default = "ghcr.io/vertex/vertex-frontend:v0.1.0"
}
variable "domain" {
  type    = string
  default = "vertex.local"
}
variable "jwt_key" {
  type      = string
  sensitive = true
}
variable "postgres_password" {
  type      = string
  sensitive = true
}
