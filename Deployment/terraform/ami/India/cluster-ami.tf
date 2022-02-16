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
  default = "ap-south-1"
}


provider "aws" {
  region = var.region
}

resource "aws_ami_copy" "india" {
  name              = "clusterAMI"
  description       = "A copy of Cluster V1"
  source_ami_id     = "ami-0df8a7ccbdf225a18"
  source_ami_region = "ap-southeast-1"

  tags = {
    Name = "Cluster V1"
  }
}