resource "kubernetes_service" "signalling" {
  metadata {
    name = "signalling"

    labels = {
      app = "signalling"
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

