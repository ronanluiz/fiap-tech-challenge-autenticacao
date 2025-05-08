terraform {
  backend "s3" {
    bucket = "tech-challenge-soat10-fase3"
    key    = "terraform/lambda/terraform.tfstate"
    region = "us-east-1"
  }
}
