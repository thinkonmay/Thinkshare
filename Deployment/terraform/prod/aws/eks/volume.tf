

resource "aws_ebs_volume" "caching-block" {
  availability_zone = "ap-southeast-1b"
  size              = 10
  type              = "gp2"

  tags = {
    Name = "Caching volume"
  }
}


resource "aws_ebs_volume" "database-block" {
  availability_zone = "ap-southeast-1b"
  size              = 50
  type              = "gp2"

  tags = {
    Name = "Caching volume"
  }
}

resource "aws_ebs_volume" "logging-block" {
  availability_zone = "ap-southeast-1b"
  size              = 50
  type              = "gp2"

  tags = {
    Name = "Caching volume"
  }
}


output "caching_volume_id" {
  value = aws_ebs_volume.caching-block.id
}
output "database_volume_id" {
  value = aws_ebs_volume.database-block.id
}
output "logging_volume_id" {
  value = aws_ebs_volume.logging-block.id
}
