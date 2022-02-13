terraform {
  required_version = ">= 0.13.1"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 3.51"
    }
  }
}

variable "region" {
  default = "ap-southeast-1"
}


provider "aws" {
  region = var.region
}

resource "aws_ami_from_instance" "cluster_dev" {
  name               = "cluser_dev"
  source_instance_id = "i-012daf26780120bd7"
}

resource "aws_ami_from_instance" "cluster_v1" {
  name               = "cluster_v1"
  source_instance_id = "i-0b2752f50102de004"
}
