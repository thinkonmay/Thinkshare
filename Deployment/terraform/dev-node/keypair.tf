resource "tls_private_key" "this" {
  algorithm = "RSA"
}

module "key_pair" {
  source = "terraform-aws-modules/key-pair/aws"

  key_name   = "cluster-keypair"
  public_key = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQDPhbeqOJOuzXVW+NUA+u26CIJcrLj1mTWjXpB/G3tCOOB6LZaQh8Ik8kb3SPnmorNQw8yIIfZI7YJgay5q7pvbrSSskZEKJa6HgTLLDqeFr9Wn/yDwjOYV9ZyYSQiGuXtDgcpT82DzmTOd3MotkTFDhQKvrDas/g4nvRWmmUlYvEw+Yh3x/Qnes/0y8/dlRUoLy3kD4gUqH/XthvfEx0wPFffwmrdoOcmM91C5knYPeVTyeLhXE6jJGLGR5pp75z2BQaDqpR0ZsILw1tYp4ZSOwIAaG+c/GZACCKPIXEdiyXPQ7DHXve/Bu8K2ghvo+8PEp1kZC4tWAGzYuyu/VhigXa/2YHZE+geoHS8lo/WOBxpgsFSLsUNpNKXxrx9vTlKtRbcox01rDw9z8z3KOz8e/jKAwRzPWjf1gGaDr91R8FY/5fpKWqczC/T67eOrINfBGi28dVVMe6iaE9IdTIbgQO8VmIze5c3WyyKxd0ZQ79HNXM/J+f7kTs4wXx6Uu5k= huyhoang@macrobuntu"
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
