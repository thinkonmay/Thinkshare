resource "tls_private_key" "this" {
  algorithm = "RSA"
}

module "key_pair" {
  source = "terraform-aws-modules/key-pair/aws"

  key_name   = "cluster_key"

  public_key = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQDW8CN3+gJrEQk0KfW3SU24QFZVxBChO5kOCNeK6gsTy+1KaIw9AJ3qv2+hoPUp3FvE8CfaiIO6az0CylZH3n/pmH1juYpo5SFeIbdHabCFtcIcHM2Uaf/HpVTrKYKfpMmBBMzvFdvTHez42/Bd6DaQEUp9G7pEzzeyZRTEoL+rHoxb21dJddqZEZ0YLy2WdhSBvvQIqXnjoyfem7GGOSic+dKlOwZrvhsnzAwnmHSMcuxyEpbR9WESwKTmE4/EEcb4FXVin0EyBkcnPIdDFHOLwZAhqQBayX4egdsfmGEqZtQQpUmx+Zjq6IChhdYGHvfGUSoMLZxxQEKZ5324OfVCPsew/tgfLH6oX99XTLYrX3VNmBSyxCWE1YMrAGizEKXcdcMQV+XsuUh+9Y9tOd0/pEEZ9Zr7Uo30lTjtBbTNpYQsH5U2kBIypctKgt43cwh54FDzh3nRUfNagH+7SVVOPj3rVPDOY8Lil4Vh2tG5hlgh2jH4FfBMaJDOm7wcn5s= huyhoang@macrobuntu"
}







output "key_pair_key_name" {
  description = "The key pair name."
  value       = module.key_pair.key_pair_key_name
}

