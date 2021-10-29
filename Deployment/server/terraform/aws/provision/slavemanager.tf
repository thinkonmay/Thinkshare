resource "kubernetes_service" "slavemanager" {
  metadata {
    name = "slavemanager"

    labels = {
      app = "slavemanager"
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
              cpu = "700m"

              memory = "1Gi"
            }

            requests = {
              cpu = "500m"

              memory = "700Mi"
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

