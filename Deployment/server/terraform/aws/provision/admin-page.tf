resource "kubernetes_service" "admin_page" {
  metadata {
    name = "admin-page"

    labels = {
      app = "admin-page"
    }
  }

  spec {
    port {
      port = 80
    }

    selector = {
      app = "admin-page"
    }

    type = "ClusterIP"
  }
}

resource "kubernetes_deployment" "admin_page" {
  metadata {
    name = "admin-page"
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "admin-page"
      }
    }

    template {
      metadata {
        labels = {
          app = "admin-page"
        }
      }

      spec {
        container {
          name  = "admin-page"
          image = "pigeatgarlic/admin-page:latest"

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

          image_pull_policy = "Always"
        }
      }
    }

    strategy {
      type = "Recreate"
    }
  }
}

