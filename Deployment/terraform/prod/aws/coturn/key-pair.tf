resource "tls_private_key" "this" {
  algorithm = "RSA"
}

module "key_pair" {
  source = "terraform-aws-modules/key-pair/aws"

  key_name   = "deployer-one"
  public_key = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQDPpv8ONTT7/AATJn8BbQYhaM5cA9ka7QR5bnCfG7PpeZdFSHpEYTNYHPnAseGyXt//e08+q9ouN9lnEmmy/0iex1xHVEpK5mM/4oMbYFq9ouwsL536Z9ICCri4FOtO5Jyh1Lg7NUhtYBMN7h/LewvwsI/hPRcVrU02vEOMUFcihNedJhYumJQZVjyPqNaQgG21tj4Xew/JPli3PUCIDRm54lfL9GtjlKSonnFrbwpUP0Cg1MBfO1EJe9eJ2609ZXD1TDoi/cq0kvdX/ODhmiFrOoHWX4otkpc+SKmfd/+Cpb7aDbrcp0wPRyWiqtIFRXFC+U+LUSp9sPXoNqpwSk7Hx/Z0CXc+oVV0B7XfzlrE0SO2wJ1FwnN18d2S20Zmj8ZeKP/WFfb+xgqLC6nxihICmWTIE8EL1gaSaK+TbHCkZCKvbZLZxK4cVkhygcd1pDFmTP3Rf5ekes51cNLi2fzs5yVReTsQ5LH57oTPzLjmQ5Y0ZZW9sG6qOX81nZCPkWM= huyhoang@macrobuntu"
}







output "key_pair_key_name" {
  description = "The key pair name."
  value       = module.key_pair.key_pair_key_name
}

output "key_pair_key_pair_id" {
  description = "The key pair ID."
  value       = module.key_pair.key_pair_key_pair_id
}

output "key_pair_fingerprint" {
  description = "The MD5 public key fingerprint as specified in section 4 of RFC 4716."
  value       = module.key_pair.key_pair_fingerprint
}
