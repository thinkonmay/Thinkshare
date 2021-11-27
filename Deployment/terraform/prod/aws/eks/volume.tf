
resource "aws_ebs_volume" "logging-block" {
  availability_zone = "ap-southeast-1b"
  size              = 30
  type              = "gp2"

  tags = {
    Name = "Logging volume"
  }
}

resource "aws_ebs_volume" "caching-block" {
  availability_zone = "ap-southeast-1b"
  size              = 5
  type              = "gp2"

  tags = {
    Name = "Caching volume"
  }
}



















output "logging_volume_id" {
  description = "volume id of ebs volume"
  value       = aws_ebs_volume.logging-block.id
}

output "caching_volume_id" {
  description = "volume id of ebs volume"
  value       = aws_ebs_volume.caching-block.id
}