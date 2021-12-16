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


resource "aws_instance" "name" {
  ami           = data.aws_ami.ubuntu.id
  instance_type = "t3.micro"
  key_name = module.key_pair.key_pair_key_name

  tags = {
    Name = "cluster manager"
  }
}

module "ec2_instance" {
  source  = "terraform-aws-modules/ec2-instance/aws"
  version = "~> 3.0"

  for_each = toset(["one"])

  name = "instance-${each.key}"

  ami                    = data.aws_ami.gaming_window.id
  associate_public_ip_address = true
  availability_zone =    "${var.region}a"
  
  instance_type          = "g4dn.xlarge"
  key_name = module.key_pair.key_pair_key_name
  monitoring             = true


  tags = {
      "Name": "Demo thinkmay worker node"
  }
}


resource "aws_security_group" "security_group" {

  name_prefix = "all_worker_management"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port = 22
    to_port   = 22
    protocol  = "tcp"

    cidr_blocks = [
      "0.0.0.0/0",
    ]
  }
  tags = {
      "Name" : "demo worker node management group"
  }
}
