
variable "region" {
  default     = "ap-southeast-1"
  description = "AWS region"
}

variable "cluster_name" {
  default = "thinkmay-eks"
}

variable "database_name" {
  default =  "thinkmaydb"
  
}

variable "map_accounts" {
  description = "Additional AWS account numbers to add to the aws-auth configmap."
  type        = list(string)

  default = [ "574816112352" ]
}

variable "map_roles" {
  description = "Additional IAM roles to add to the aws-auth configmap."
  type = list(object({
    rolearn  = string
    username = string
    groups   = list(string)
  }))

  default = [
    {
      rolearn  = "arn:aws:iam::574816112352:role/thinkmay"
      username = "thinkmay"
      groups   = ["system:masters"]
    }
  ]
}

variable "map_users" {
  description = "Additional IAM users to add to the aws-auth configmap."
  type = list(object({
    userarn  = string
    username = string
    groups   = list(string)
  }))

  default = [
    {
      userarn  = "arn:aws:iam::574816112352:user/thinkmay"
      username = "thinkmay"
      groups   = ["system:masters"]
    }
  ]
}


