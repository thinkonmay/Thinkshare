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
    values = ["cluster_v1"]
  }


  owners = ["574816112352"] # Canonical
}

