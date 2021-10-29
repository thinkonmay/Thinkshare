resource "kubernetes_service" "conductor" {
  metadata {
    name = "conductor"

    labels = {
      app = "conductor"
    }
  }

  spec {
    port {
      port = 80
    }

    selector = {
      app = "conductor"
    }

    type = "ClusterIP"
  }
}

resource "kubernetes_deployment" "conductor" {
  metadata {
    name = "conductor"
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "conductor"
      }
    }

    template {
      metadata {
        labels = {
          app = "conductor"
        }
      }

      spec {
        container {
          name  = "conductor"
          image = "pigeatgarlic/conductor:release_latest"

          port {
            container_port = 80
          }

          resources {
            limits = {
              cpu = "500m"

              memory = "700Mi"
            }

            requests = {
              cpu = "300m"

              memory = "500Mi"
            }
          }

          image_pull_policy = "Always"
        }
      }
    }

    strategy {
      type = "Recreate"
    }
  }
}

