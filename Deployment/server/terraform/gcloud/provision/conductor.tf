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
              cpu = "300m"

              memory = "300Mi"
            }

            requests = {
              cpu = "200m"

              memory = "200Mi"
            }
          }

          readiness_probe {
            http_get {
              path = "/swagger/index.html"
              port = "80"
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

