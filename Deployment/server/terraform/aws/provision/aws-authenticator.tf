module "ec2-instance" {
  source  = "terraform-aws-modules/ec2-instance/aws"
  version = "3.2.0"
  # insert the 34 required variables here

  instance_types = "t3.micro"

}