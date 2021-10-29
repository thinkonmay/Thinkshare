resource "kubernetes_config_map" "dbmanager_config" {
  metadata {
    name = "dbmanager-config"

    labels = {
      app = "dbmanager"
    }
  }

  data = {
    PGADMIN_DEFAULT_EMAIL = "admin@thinkmay.com"

    PGADMIN_DEFAULT_PASSWORD = "ASDFak!C#$%2351531c2c152"
  }
}

