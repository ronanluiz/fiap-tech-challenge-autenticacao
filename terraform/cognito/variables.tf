# variables.tf
locals {
  project = "${var.environment}-${var.project_name}"
}

variable "region" {
  description = "A região da AWS onde os recursos serão criados"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Ambiente no qual o projeto será implantado"
  type        = string
  default     = "dev"
}

variable "project_name" {
  type    = string
  default = "tc-cognito"
}


