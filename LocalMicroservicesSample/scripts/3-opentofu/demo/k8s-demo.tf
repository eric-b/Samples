data "kubectl_path_documents" "demo_ns" {
    pattern = "../../demo/k8s/demo-namespace.yaml"
    vars = {
        namespace = var.demo-namespace
    }
}

resource "kubectl_manifest" "demo_ns" {
    for_each  = data.kubectl_path_documents.demo_ns.manifests
    yaml_body = each.value
}

data "kubectl_path_documents" "demo_manifests" {
    pattern = "../../demo/k8s/**/*.yaml"
    vars = {
        namespace = var.demo-namespace
        sql_db_password = var.sqlserver_demo_container_password
    }
}

resource "kubectl_manifest" "apply_demo" {
    for_each  = data.kubectl_path_documents.demo_manifests.manifests
    yaml_body = each.value
}