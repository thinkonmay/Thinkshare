resource "tls_private_key" "this" {
  algorithm = "RSA"
}

module "key_pair" {
  source = "terraform-aws-modules/key-pair/aws"

  key_name   = "Tet-keypair"
  public_key = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQDnXzTC4PAtx6GrttUKqXD4doC0PdhGf4N8g1HqfoAMY8WOfkbdwLW3sZJNaSF/LMlmDACvtvqy6XSTypUxIObnNbMRKZf8jJWXeWOJqSqCKmJURqr+POfkBaiR0ViT8hUvKAPVoMLNVgfXr25rjqbskA5F+r9CU/U+UjbhsH9i7dh/KsJece6Sz5R03IHoNRV9QlrDqPUrsNV4dhrpv4J+9r8jWvU5lyIZYneOlvmNuvWBhW2jjLGmN9ukcKWz51zIvfK9n7lAgyIYgwXAkjeDCyvBLCzH615Q58tAiSummxprS8R2wePQtxF9F4JyWKdUAiwAiaTStWBOw4biRAt4SKU2b+fz5hSnMBtYDtGAJ/EV4NrVFU2rb3aOpbHRllp/X79Chs2p9KveOFxNsh6wPHMR/DzZdyEvWVchtY3wNLAtExAjxUY3ZdxUMroInPYt+ieaodTUrHDH8WIyY3GKjIHnlNh0iU5BFAAuG3LT/off3/RvJ0niaD3skIj9bHk= hoang@microbuntu"
}







output "key_pair_key_name" {
  description = "The key pair name."
  value       = module.key_pair.key_pair_key_name
}

