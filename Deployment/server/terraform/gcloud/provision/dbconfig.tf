resource "kubernetes_config_map" "systemdb_config" {
  metadata {
    name = "systemdb-config"

    labels = {
      app = "systemdb"
    }
  }

  data = {
    POSTGRES_DB = "systemdb"

    POSTGRES_PASSWORD = "thinkmayvantue"

    POSTGRES_USER = "thinkmay"
  }
}

