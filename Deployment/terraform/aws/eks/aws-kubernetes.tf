terraform {
  required_version = ">= 0.12.0"
}

provider "kubernetes" {
  host                   = data.aws_eks_cluster.cluster.endpoint
  cluster_ca_certificate = base64decode(data.aws_eks_cluster.cluster.certificate_authority.0.data)
  token                  = data.aws_eks_cluster_auth.cluster_auth.token
}

provider "aws" {
  region  = var.region
}

data "aws_eks_cluster" "cluster" {
  name = module.eks.cluster_id
}

data "aws_eks_cluster_auth" "cluster_auth"{
  name = module.eks.cluster_id
}

data "aws_availability_zones" "available" {
}




module "eks" {
  source       = "terraform-aws-modules/eks/aws"

  cluster_name    = var.cluster_name
  cluster_version = "1.20"

  subnets         = module.vpc.private_subnets
  cluster_create_timeout = "1h"

  cluster_endpoint_private_access = true
  cluster_endpoint_public_access  = true

  vpc_id = module.vpc.vpc_id



  # Managed Node Groups
  node_groups_defaults = {
    ami_type  = "AL2_x86_64"
    disk_size = 10
  }

  node_groups = {
    # cluster_node = {
    #   desired_capacity = 0
    #   max_capacity     = 1
    #   min_capacity     = 0

    #   instance_types = ["t3.micro"]
    #   disk_size      = 5
    #   capacity_type  = "SPOT"

    #   create_launch_template = true
    #   public_ip      = true
      
    #   k8s_labels = {
    #     Environment = "production"
    #     Type        = "Cluster-resources"
    #   }

    #   update_config = {
    #     max_unavailable_percentage = 50 # or set `max_unavailable`
    #   }
    # },


    thinkmay_node = {
      desired_capacity = 6
      max_capacity     = 8
      min_capacity     = 4

      instance_types = ["t3.medium"]
      capacity_type  = "SPOT"
      
      k8s_labels = {
        Environment = "production"
        Color = "BLUE"
      }
      update_config = {
        max_unavailable_percentage = 0 # or set `max_unavailable`
      }
    }

    requested_node_2 = {
      desired_capacity = 2
      max_capacity     = 2
      min_capacity     = 2

      instance_types = ["t3.medium"]
      capacity_type  = "ON_DEMAND"
      aws_availability_zones = "ap-southeast-1b"
      
      k8s_labels = {
        Environment = "production"
        Color = "RED"
      }
      update_config = {
        max_unavailable_percentage = 100 # or set `max_unavailable`
      }
    }
    requested_node = {
      desired_capacity = 2
      max_capacity     = 2
      min_capacity     = 2

      instance_types = ["t3.medium"]
      capacity_type  = "ON_DEMAND"
      
      k8s_labels = {
        Environment = "production"
        Color = "RED"
      }
      update_config = {
        max_unavailable_percentage = 100 # or set `max_unavailable`
      }
    }
  }

  

  worker_additional_security_group_ids = [aws_security_group.all_worker_mgmt.id]
  map_users                            = var.map_users
  map_accounts                         = var.map_accounts
  map_roles                            = var.map_roles 
}







//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
# output "kubectl_config" {
#   description = "kubectl config as generated by the module."
#   value       = module.eks.kubeconfig
# }

