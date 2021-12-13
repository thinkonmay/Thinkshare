module "vpc" {
  source  = "terraform-aws-modules/vpc/aws"

  name                 = "vpc"
  cidr                 = "10.0.0.0/16"
  azs                  = ["ap-southeast-1a","ap-southeast-1b"]
  private_subnets      = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
  public_subnets       = ["10.0.4.0/24", "10.0.5.0/24", "10.0.6.0/24"]
  enable_nat_gateway   = true
  single_nat_gateway   = true
  enable_dns_hostnames = true

  public_subnet_tags = {
    "kubernetes.io/cluster/${var.cluster_name}" = "shared"
    "kubernetes.io/role/elb"                      = "1"
  }

  private_subnet_tags = {
    "kubernetes.io/cluster/${var.cluster_name}" = "shared"
    "kubernetes.io/role/internal-elb"             = "1"
  }
}


resource "aws_security_group" "all_worker_mgmt" {
  name_prefix = "all_worker_management"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port = 22
    to_port   = 22
    protocol  = "tcp"

    cidr_blocks = [
      "10.0.0.0/8",
      "172.16.0.0/12",
      "192.168.0.0/16",
    ]
  }
}

data "tls_certificate" "cluster" {
  url = module.eks.cluster_oidc_issuer_url
}

resource "aws_iam_openid_connect_provider" "cluster" {
  client_id_list  = ["sts.amazonaws.com"]
  thumbprint_list = concat([data.tls_certificate.cluster.certificates.0.sha1_fingerprint])
  url             = module.eks.cluster_oidc_issuer_url
}

module "alb_ingress_controller" {
  source  = "iplabs/alb-ingress-controller/kubernetes"


  k8s_cluster_type = "eks"
  k8s_namespace    = "kube-system"

  aws_region_name  = var.region
  k8s_cluster_name = var.cluster_name
  aws_vpc_id       = module.vpc.vpc_id
}
