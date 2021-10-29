resource "kubernetes_service" "landing_page" {
  metadata {
    name = "landing-page"

    labels = {
      app = "landing-page"
    }
  }

  spec {
    port {
      port = 80
    }

    selector = {
      app = "landing-page"
    }

    type = "ClusterIP"
  }
}

resource "kubernetes_deployment" "landing_page" {
  metadata {
    name = "landing-page"
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "landing-page"
      }
    }

    template {
      metadata {
        labels = {
          app = "landing-page"
        }
      }

      spec {
        container {
          name  = "landing-page"
          image = "pigeatgarlic/landing-page:latest"

          port {
            container_port = 80
          }

          resources {
            limits = {
              cpu = "300m"

              memory = "700Mi"
            }

            requests = {
              cpu = "200m"

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

