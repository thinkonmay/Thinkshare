data "aws_ami" "ubuntu" {
  most_recent = true

  filter {
    name   = "name"
    values = ["cluster_v1"]
  }


  owners = ["574816112352"] # Canonical
}

