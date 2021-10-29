resource "kubernetes_persistent_volume_claim" "logging_volume_claim" {
  metadata {
    name = "logging-volume-claim"
  }

  spec {
    access_modes = ["ReadWriteMany"]

    resources {
      requests = {
        storage = "30Gi"
      }
    }
  }
}


