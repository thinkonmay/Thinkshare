resource "kubernetes_service" "dbmanager" {
  metadata {
    name = "dbmanager"

    labels = {
      app = "dbmanager"
    }
  }

  spec {
    port {
      port = 80
    }

    selector = {
      app = "dbmanager"
    }

    type = "ClusterIP"
  }
}

resource "kubernetes_deployment" "dbmanager" {
  metadata {
    name = "dbmanager"
  }

  spec {
    selector {
      match_labels = {
        app = "dbmanager"
      }
    }

    template {
      metadata {
        labels = {
          app = "dbmanager"
        }
      }

      spec {
        container {
          name  = "dbmanager"
          image = "dpage/pgadmin4"

          port {
            container_port = 80
          }

          env_from {
            config_map_ref {
              name = "dbmanager-config"
            }
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

          image_pull_policy = "IfNotPresent"
        }
      }
    }

    strategy {
      type = "Recreate"
    }
  }
}

