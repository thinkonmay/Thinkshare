resource "tls_private_key" "this" {
  algorithm = "RSA"
}

module "key_pair" {
  source = "terraform-aws-modules/key-pair/aws"

  key_name   = "cluster-keypair"
  public_key = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC+Zp0+3dsYnsecKnAS+ZZDpPc7wsnXBxwki8UMKR7zzfxTMjOyL/ZxzPIzToh3X+O7K+E+KzaWvgUMef5CG/IIliTdL34FjTtdd7g60d2fyUcuM+kf9YccXjYxt/TJG7biGVGyoAhu1U27iMEiuFp/LPcVNA2ri7EIqxhUOfCTWi8vwtauYYbl906DuzORUEMSRe2QALh5o0nqxzDH1zIJ8EjkSRgKYfXJKm0lEipIMPDBDLrrGzQsUcXJtTaGjC8eqivnkl/xpOHz2gW9UlnKK7RfCdaA1enC/zAZ10rhh7Ghaw0Fjlauq59ZT18LACY/j7SxviP+2ohECLQICEacbAzrVR0Ayk3oRx59osQwIxL5Rjs16Egb+4MLpK5A9ei+1LS3u9rG8lxd5LZ01kFYeEJrZPQCfK2ru2X4ctHfLCzfBKN+x/2Xhkd/Zj44MoMdzavKs7FA/nQFYxumm3JdfKHjaHkaZa9La9RZr+TQckYO8ewWno4ZwTNHuZgpZ2M= hoang@microbuntu"
  # public_key = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQDPhbeqOJOuzXVW+NUA+u26CIJcrLj1mTWjXpB/G3tCOOB6LZaQh8Ik8kb3SPnmorNQw8yIIfZI7YJgay5q7pvbrSSskZEKJa6HgTLLDqeFr9Wn/yDwjOYV9ZyYSQiGuXtDgcpT82DzmTOd3MotkTFDhQKvrDas/g4nvRWmmUlYvEw+Yh3x/Qnes/0y8/dlRUoLy3kD4gUqH/XthvfEx0wPFffwmrdoOcmM91C5knYPeVTyeLhXE6jJGLGR5pp75z2BQaDqpR0ZsILw1tYp4ZSOwIAaG+c/GZACCKPIXEdiyXPQ7DHXve/Bu8K2ghvo+8PEp1kZC4tWAGzYuyu/VhigXa/2YHZE+geoHS8lo/WOBxpgsFSLsUNpNKXxrx9vTlKtRbcox01rDw9z8z3KOz8e/jKAwRzPWjf1gGaDr91R8FY/5fpKWqczC/T67eOrINfBGi28dVVMe6iaE9IdTIbgQO8VmIze5c3WyyKxd0ZQ79HNXM/J+f7kTs4wXx6Uu5k= huyhoang@macrobuntu"
}







output "key_pair_key_name" {
  description = "The key pair name."
  value       = module.key_pair.key_pair_key_name
}

