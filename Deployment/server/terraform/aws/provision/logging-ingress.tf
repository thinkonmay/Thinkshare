resource "kubernetes_ingress" "logging_ingress" {
  metadata {
    name      = "logging-ingress"
    namespace = "kube-logging"

    annotations = {
      "kubernetes.io/ingress.class" = "alb"
    }
  }

  spec {
    tls {
      hosts       = ["logging.thinkmay.net"]
      secret_name = "secret-tls"
    }

    rule {
      host = "logging.thinkmay.net"

      http {
        path {
          path = "/"
        }
      }
    }
  }
}

