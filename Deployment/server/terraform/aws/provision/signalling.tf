resource "kubernetes_service" "signalling" {
  metadata {
    name = "signalling"

    labels = {
      app = "signalling"
    }
  }

  spec {
    port {
      port = 80
    }

    selector = {
      app = "signalling"
    }

    type = "ClusterIP"
  }
}

resource "kubernetes_deployment" "signalling" {
  metadata {
    name = "signalling"
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "signalling"
      }
    }

    template {
      metadata {
        labels = {
          app = "signalling"
        }
      }

      spec {
        container {
          name  = "signalling"
          image = "pigeatgarlic/signalling:release_latest"

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

