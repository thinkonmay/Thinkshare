resource "google_container_cluster" "thinkmaydev" {
  name     = "thinkmaydev"
  location = var.region

  # We can't create a cluster with no node pool defined, but we want to only use
  # separately managed node pools. So we create the smallest possible default
  # node pool and immediately delete it.
  remove_default_node_pool = true
  initial_node_count       = 1
}


resource "google_container_node_pool" "primary_preemptible_nodes" {
  name       = "dev-node-pool"
  location   = var.region
  cluster    = google_container_cluster.thinkmaydev.name
  initial_node_count =       1

  node_config {
    preemptible  = true
    machine_type = "e2-medium"
  }
}


output "kubernetes_cluster_host" {
  value       = google_container_cluster.thinkmaydev.endpoint
  description = "GKE Cluster Host"
}

variable "gke_master_ipv4_cidr_block" {
  type    = string
  default = "172.23.0.0/28"
}