resource "kubernetes_service" "slavemanager" {
  metadata {
    name = "slavemanager"

    labels = {
      app = "slavemanager"
    }

    annotations = {
      "cloud.google.com/backend-config" = "{\"ports\": {\"80\":\"no-timeout\"}}"
    }
  }

  spec {
    port {
      port = 80
    }

    selector = {
      app = "slavemanager"
    }

    type = "ClusterIP"
  }
}

resource "kubernetes_deployment" "slavemanager" {
  metadata {
    name = "slavemanager"
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "slavemanager"
      }
    }

    template {
      metadata {
        labels = {
          app = "slavemanager"
        }
      }

      spec {
        container {
          name  = "slavemanager"
          image = "pigeatgarlic/slavemanager:release_latest"

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

