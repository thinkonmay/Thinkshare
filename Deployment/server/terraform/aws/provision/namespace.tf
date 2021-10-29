resource "kubernetes_namespace" "kube_logging" {
  metadata {
    name = "kube-logging"
  }
}

