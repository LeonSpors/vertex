variable "namespace" { type = string }
variable "chart_path" { type = string }
variable "api_image" { type = string }
variable "frontend_image" { type = string }
variable "domain" { type = string }
variable "jwt_key" {
  type = string
  sensitive = true
}
