module "vpc" {
  source  = "terraform-aws-modules/vpc/aws"

  name                 = "vpc"
  cidr                 = "10.0.0.0/16"
  azs                  = ["${var.region}a"]
  public_subnets       = ["10.0.4.0/24"]
  enable_nat_gateway   = false
  single_nat_gateway   = false
  enable_dns_hostnames = false
}