terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  required_version = "~> 1.11.3"
}

provider "aws" {
  region = var.regiao

  default_tags {
    tags = {
      terraform = "true"
    }
  }
}