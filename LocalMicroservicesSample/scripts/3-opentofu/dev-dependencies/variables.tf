variable "dev-dependencies-namespace" {
  description = "Namespace of dev dependencies in K8s"
  type        = string
  default     = "dev-dependencies"
}

variable "azurite_hostpath" {
  description = "Path of azurite workspace on host. Example: /run/desktop/mnt/host/c/tmp/azurite-docker"
  type        = string
}

variable "sqlserver_sa_password" {
  description = "Database super admin password (required by Azure Service Bus Emulator)"
  type        = string
  sensitive   = true
}

