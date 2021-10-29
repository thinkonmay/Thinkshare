# resource "kubernetes_ingress" "default_ingress" {
#   metadata {
#     name = "default-ingress"

#     annotations = {
#       "kubernetes.io/ingress.class" = "alb"
#     }
#   }

#   spec {
#     rule {
#       host = "conductor.thinkmay.net"

#       http {
#         path {
#           path = "/"
#         }
#       }
#     }

#     rule {
#       host = "signalling.thinkmay.net"

#       http {
#         path {
#           path = "/"
#         }
#       }
#     }

#     rule {
#       host = "slavemanager.thinkmay.net"

#       http {
#         path {
#           path = "/"
#         }
#       }
#     }

#     rule {
#       host = "thinkmay.net"

#       http {
#         path {
#           path = "/"
#         }
#       }
#     }

#     rule {
#       host = "service.thinkmay.net"

#       http {
#         path {
#           path = "/"
#         }
#       }
#     }

#     rule {
#       host = "admin.thinkmay.net"

#       http {
#         path {
#           path = "/login"
#         }
#       }
#     }

#     rule {
#       host = "database.thinkmay.net"

#       http {
#         path {
#           path = "/"
#         }
#       }
#     }

#     rule {
#       host = "resources.thinkmay.net"

#       http {
#         path {
#           path = "/"
#         }
#       }
#     }
#   }
# }

