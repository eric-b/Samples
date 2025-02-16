data "kubectl_path_documents" "devdeps_ns" {
    pattern = "../../dev-dependencies/dev-dependencies-namespace.yaml"
    vars = {
        namespace = var.dev-dependencies-namespace
    }
}

resource "kubectl_manifest" "apply_devdeps_ns" {
    for_each  = data.kubectl_path_documents.devdeps_ns.manifests
    yaml_body = each.value
}

data "kubectl_path_documents" "devdeps_manifests" {
    pattern = "../../dev-dependencies/**/*.yaml"
    vars = {
        namespace = var.dev-dependencies-namespace
        MSSQL_SA_PASSWORD = var.sqlserver_sa_password
        AZURITE_HOST_PATH = var.azurite_hostpath
    }
}

resource "kubectl_manifest" "apply_core" {
    for_each  = data.kubectl_path_documents.devdeps_manifests.manifests
    yaml_body = each.value
}