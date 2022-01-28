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

resource "aws_ebs_volume" "root" {
  availability_zone = "${var.region}a"
  size              = 50
}



module "ec2_instance" {
  source  = "terraform-aws-modules/ec2-instance/aws"
  version = "~> 3.0"


  name = "ultrabuntu"

  ami                    = data.aws_ami.ubuntu.id
  associate_public_ip_address = true
  availability_zone =    "${var.region}a"
  
  instance_type          = "c5a.xlarge"
  key_name = module.key_pair.key_pair_key_name
  monitoring             = true
  
  root_block_device = [{
    delete_on_termination = false
    device_name = "/dev/sda1"
    volume_id = aws_ebs_volume.root.id
  }]


  tags = {
      "Name": "Thinkmay dev node"
  }
}

output "ip" {
  value = module.ec2_instance.public_ip
}


