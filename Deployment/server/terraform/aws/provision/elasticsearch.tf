resource "kubernetes_stateful_set" "es_cluster" {
  metadata {
    name      = "es-cluster"
    namespace = "kube-logging"
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "elasticsearch"
      }
    }

    template {
      metadata {
        labels = {
          app = "elasticsearch"
        }
      }

      spec {
        volume {
          name = "data"

          persistent_volume_claim {
            claim_name = "logging-volume-claim"
          }
        }

        init_container {
          name    = "fix-permissions"
          image   = "busybox"
          command = ["sh", "-c", "chown -R 1000:1000 /usr/share/elasticsearch/data"]

          volume_mount {
            name       = "data"
            mount_path = "/usr/share/elasticsearch/data"
          }

          security_context {
            privileged = true
          }
        }

        init_container {
          name    = "increase-vm-max-map"
          image   = "busybox"
          command = ["sysctl", "-w", "vm.max_map_count=262144"]

          security_context {
            privileged = true
          }
        }

        init_container {
          name    = "increase-fd-ulimit"
          image   = "busybox"
          command = ["sh", "-c", "ulimit -n 65536"]

          security_context {
            privileged = true
          }
        }

        container {
          name  = "elasticsearch"
          image = "docker.elastic.co/elasticsearch/elasticsearch:7.2.0"

          port {
            name           = "rest"
            container_port = 9200
            protocol       = "TCP"
          }

          port {
            name           = "inter-node"
            container_port = 9300
            protocol       = "TCP"
          }

          env {
            name  = "cluster.name"
            value = "k8s-logs"
          }

          env {
            name = "node.name"

            value_from {
              field_ref {
                field_path = "metadata.name"
              }
            }
          }

          env {
            name  = "discovery.seed_hosts"
            value = "es-cluster-0.elasticsearch"
          }

          env {
            name  = "cluster.initial_master_nodes"
            value = "es-cluster-0"
          }

          env {
            name  = "ES_JAVA_OPTS"
            value = "-Xms512m -Xmx512m"
          }

          resources {
            limits = {
              cpu = "1500m"

              memory = "3Gi"
            }

            requests = {
              cpu = "1"

              memory = "2Gi"
            }
          }

          volume_mount {
            name       = "data"
            mount_path = "/usr/share/elasticsearch/data"
          }
        }
      }
    }

    service_name = "elasticsearch"
  }
}

