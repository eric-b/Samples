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
    helm = {
      version = ">= 2.17.0"
    }
  }
}

provider "kubectl" {
  load_config_file = true
}

provider "helm" {
  kubernetes {
    config_path = "~/.kube/config"
  }
}