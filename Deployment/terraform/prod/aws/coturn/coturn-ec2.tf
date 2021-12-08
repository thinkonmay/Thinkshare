provider "aws" {
  region = "ap-southeast-1"
}

terraform {
  required_version = ">= 0.12.6"

  required_providers {
    aws    = ">= 2.46"
    random = ">= 2.0"
    tls    = ">= 1.0"
  }
}


data "aws_ami" "ubuntu" {
  most_recent = true

  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-focal-20.04-amd64-server-*"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }

  owners = ["099720109477"] # Canonical
}



resource "aws_instance" "coturn" {
  ami           = data.aws_ami.ubuntu.id
  instance_type = "t3.micro"

#   key_name = module.key_pair.key_pair_key_name

  tags = {
    Name = "coturn"
  }
}

output "coturn_ip" {
    description = "ip address of coturn ip"
    value = aws_instance.coturn.public_ip
}