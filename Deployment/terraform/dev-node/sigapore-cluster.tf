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

resource "aws_volume_attachment" "ebs_att" {
  device_name = "/dev/sdt"
  volume_id   = aws_ebs_volume.epitchi.id
  instance_id = module.ec2_instance.id
}

output "ip" {
  value = module.ec2_instance.public_ip
}

resource "aws_ebs_volume" "epitchi" {
  availability_zone =    "${var.region}a"
  size              = 10
}

resource "aws_ebs_volume" "root" {
  availability_zone =    "${var.region}a"
  size              = 20
}

