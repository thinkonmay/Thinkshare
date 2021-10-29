resource "kubernetes_service" "service_page" {
  metadata {
    name = "service-page"

    labels = {
      app = "service-page"
    }
  }

  spec {
    port {
      port = 80
    }

    selector = {
      app = "service-page"
    }

    type = "ClusterIP"
  }
}

resource "kubernetes_deployment" "service_page" {
  metadata {
    name = "service-page"
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "service-page"
      }
    }

    template {
      metadata {
        labels = {
          app = "service-page"
        }
      }

      spec {
        container {
          name  = "service-page"
          image = "pigeatgarlic/service-page:latest"

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
              path = "/login"
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

