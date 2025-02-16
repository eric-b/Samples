resource "helm_release" "reloader" {
  depends_on = [kubectl_manifest.apply_devdeps_ns]
  name       = "reloader"
  repository = "https://stakater.github.io/stakater-charts"
  chart      = "reloader"
  namespace  = var.dev-dependencies-namespace
  set {
      name  = "reloader.watchGlobally"
      value = "false"
    }
}