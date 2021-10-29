module "argocd" {
  source = "git::https://github.com/DNXLabs/terraform-aws-eks-argocd.git?ref=0.1.0"

  enabled = true
}