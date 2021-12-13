
resource "aws_ebs_volume" "caching-block" {
  availability_zone = "ap-southeast-1b"
  size              = 5
  type              = "gp2"

  tags = {
    Name = "Caching volume"
  }
}

resource "aws_security_group" "efs-security-group" {
  name_prefix = "all_worker_management"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port = 2049
    to_port   = 2049
    protocol  = "tcp"

    cidr_blocks = [
      "0.0.0.0/0",
    ]
  }
}




resource "aws_efs_file_system" "efs" {
  creation_token = "eks-volume"
  tags = {
    Name = "eks-volume"
  }
}

resource "aws_efs_mount_target" "volume" {
  file_system_id = "${aws_efs_file_system.efs.id}"

  security_groups = [
    aws_security_group.all_worker_mgmt.id,
    aws_security_group.efs-security-group.id
  ]
  
  subnet_id = "${module.vpc.public_subnets[0]}"
}

output "efs-volume-id" {
  description = "id of efs block"
  value = "${aws_efs_mount_target.volume.id}"
}
output "efs-file-system-id" {
  description = "id of efs file system"
  value = "${aws_efs_file_system.efs.id}"
}