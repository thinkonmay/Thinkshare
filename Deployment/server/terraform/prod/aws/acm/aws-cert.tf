
provider "aws" {
  region  = var.region
}
resource "aws_acm_certificate" "domain" {
  domain_name       = "thinkmay.net"
  validation_method = "EMAIL"


  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_acm_certificate" "subdomain" {
  domain_name       = "*.thinkmay.net"
  validation_method = "EMAIL"


  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_acm_certificate_validation" "domainvalid" {
  certificate_arn = aws_acm_certificate.domain.arn
}

resource "aws_acm_certificate_validation" "subdomainvalid" {
  certificate_arn = aws_acm_certificate.subdomain.arn
}
