data "aws_ami" "gaming_window" {
  most_recent = true
  owners      = ["877902723034"]

  filter {
    name   = "name"
    values = ["DCV-Windows-2021-2-11135-NVIDIA-gaming-*"]
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

