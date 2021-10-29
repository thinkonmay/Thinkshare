resource "kubernetes_ingress" "ingress" {
  metadata {
    name = "ingress"

    annotations = {
      "kubernetes.io/ingress.class" = "gce"

      "kubernetes.io/ingress.global-static-ip-name" = "thinkmay-ip"

      "networking.gke.io/managed-certificates" = "managed-cert"
    }
  }

  spec {
    rule {
      host = "conductor.thinkmay.net"

      http {
        path {
          path = "/*"
        }
      }
    }

    rule {
      host = "signalling.thinkmay.net"

      http {
        path {
          path = "/*"
        }
      }
    }

    rule {
      host = "slavemanager.thinkmay.net"

      http {
        path {
          path = "/*"
        }
      }
    }

    rule {
      host = "thinkmay.net"

      http {
        path {
          path = "/*"
        }
      }
    }

    rule {
      host = "service.thinkmay.net"

      http {
        path {
          path = "/*"
        }
      }
    }
  }
}

