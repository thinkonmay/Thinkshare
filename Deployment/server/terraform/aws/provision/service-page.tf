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
              cpu = "700m"

              memory = "1Gi"
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

