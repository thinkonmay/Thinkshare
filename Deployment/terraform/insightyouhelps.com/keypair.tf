resource "tls_private_key" "this" {
  algorithm = "RSA"
}

module "key_pair" {
  source = "terraform-aws-modules/key-pair/aws"

  key_name   = "linh-keypair"
  public_key = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQDPguPsZgD0DkATAUK8u2JIKO9tLDxGVxfdlWuhtMvqZW4mqcpK0zZqlcJgamUu/pIcC4IkCNHkafXPQZVt6/IaN8PJDf76m5RyoNw3ton+1sEi6V3Q7TQubii/2ldnfF76CjYNF7MqHDRET5MVdFSxEY8HeY8iZHLqgrx0negQV8kAZkIFqtP1ILA1ACUfc139okWMSAos01xCddxM7phb68DLSOND4LR2Vw1B+PNXeDo3DlsX9AsSoyfv8Y45kaXwhwcDFyoLczce3C8c6Kt9QbDHyZuLY7lKjj5LfJW2MG12/SCpB3wuFRgVnd820AJHSL6gsnSES5lPFX64qN0Y7WmKgTn4FIaHjIshJX53XYVo6uHx4y7lE3wcEGY2Cz1T8zO5Zq1D3/S9SMhRbF90Dq24FD9nEMrHzPWFUtIeNUYdRZ8+jJeG7dtPviD6CMVyk94aMSqUO4ATY2H+wr9ajheWA2zI1/86u+nOsGhEDVqc9B2MoTrvARY9Rf+u9xU= huyhoang@macrobuntu"
}


output "key_pair_key_name" {
  description = "The key pair name."
  value       = module.key_pair.key_pair_key_name
}