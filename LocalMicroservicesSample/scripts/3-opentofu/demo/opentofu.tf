terraform {
  required_version = ">= 0.13"

  backend "local" {
    path = ""
  }

  required_providers {
     kubectl = {
      source  = "gavinbunney/kubectl"
      version = ">= 1.19.0"
    }
  }
}

provider "kubectl" {
  load_config_file = true
}