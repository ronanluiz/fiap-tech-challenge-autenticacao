terraform {
  backend "s3" {
    bucket = "soat10-tech-challenge-fase3"
    key    = "terraform/cognito/terraform.tfstate"
    region = "us-east-1"
  }
}
