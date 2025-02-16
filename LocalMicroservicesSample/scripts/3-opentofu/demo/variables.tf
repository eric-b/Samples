variable "demo-namespace" {
  description = "Namespace of demo in k8s"
  type        = string
  default     = "demo"
}


variable "sqlserver_demo_container_password" {
  description = "Database password of user demo"
  type        = string
  sensitive   = true
}