resource "kubernetes_service" "resources" {
  metadata {
    name = "resources"

    labels = {
      app = "resources"
    }
  }

  spec {
    port {
      port = 80
    }

    selector = {
      app = "resources"
    }

    type = "ClusterIP"
  }
}

resource "kubernetes_deployment" "resources" {
  metadata {
    name = "resources"
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "resources"
      }
    }

    template {
      metadata {
        labels = {
          app = "resources"
        }
      }

      spec {
        container {
          name  = "resources"
          image = "pigeatgarlic/resources:release_latest"

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

