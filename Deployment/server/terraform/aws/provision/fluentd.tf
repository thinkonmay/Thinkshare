resource "kubernetes_service_account" "fluentd" {
  metadata {
    name      = "fluentd"
    namespace = "kube-logging"

    labels = {
      app = "fluentd"
    }
  }
}

resource "kubernetes_cluster_role" "fluentd" {
  metadata {
    name = "fluentd"

    labels = {
      app = "fluentd"
    }
  }

  rule {
    verbs      = ["get", "list", "watch"]
    api_groups = [""]
    resources  = ["pods", "namespaces"]
  }
}

resource "kubernetes_cluster_role_binding" "fluentd" {
  metadata {
    name = "fluentd"
  }

  subject {
    kind      = "ServiceAccount"
    name      = "fluentd"
    namespace = "kube-logging"
  }

  role_ref {
    api_group = "rbac.authorization.k8s.io"
    kind      = "ClusterRole"
    name      = "fluentd"
  }
}

resource "kubernetes_daemonset" "fluentd" {
  metadata {
    name      = "fluentd"
    namespace = "kube-logging"

    labels = {
      app = "fluentd"
    }
  }

  spec {
    selector {
      match_labels = {
        app = "fluentd"
      }
    }

    template {
      metadata {
        labels = {
          app = "fluentd"
        }
      }

      spec {
        volume {
          name = "varlog"

          host_path {
            path = "/var/log"
          }
        }

        volume {
          name = "varlibdockercontainers"

          host_path {
            path = "/var/lib/docker/containers"
          }
        }

        container {
          name  = "fluentd"
          image = "fluent/fluentd-kubernetes-daemonset:v1.4.2-debian-elasticsearch-1.1"

          env {
            name  = "FLUENT_ELASTICSEARCH_HOST"
            value = "elasticsearch.kube-logging.svc.cluster.local"
          }

          env {
            name  = "FLUENT_ELASTICSEARCH_PORT"
            value = "9200"
          }

          env {
            name  = "FLUENT_ELASTICSEARCH_SCHEME"
            value = "http"
          }

          env {
            name  = "FLUENTD_SYSTEMD_CONF"
            value = "disable"
          }

          env {
            name  = "FLUENT_CONTAINER_TAIL_EXCLUDE_PATH"
            value = "/var/log/containers/fluent*"
          }

          env {
            name  = "FLUENT_CONTAINER_TAIL_PARSER_TYPE"
            value = "/^(?<time>.+) (?<stream>stdout|stderr) [^ ]* (?<log>.*)$/"
          }

          resources {
            limits = {
              cpu = "100m"

              memory = "100Mi"
            }

            requests = {
              cpu = "50m"

              memory = "50Mi"
            }
          }

          volume_mount {
            name       = "varlog"
            mount_path = "/var/log"
          }

          volume_mount {
            name       = "varlibdockercontainers"
            read_only  = true
            mount_path = "/var/lib/docker/containers"
          }
        }

        termination_grace_period_seconds = 30
        service_account_name             = "fluentd"

        toleration {
          key    = "node-role.kubernetes.io/master"
          effect = "NoSchedule"
        }
      }
    }
  }
}

