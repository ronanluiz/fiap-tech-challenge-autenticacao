# variables.tf
locals {
  projeto = "${var.ambiente}-tc-cognito"
}

variable "regiao" {
  description = "A região da AWS onde os recursos serão criados"
  type        = string
  default     = "us-east-1"
}

variable "ambiente" {
  description = "Ambiente no qual o projeto será implantado"
  type        = string
  default     = "dev"
}


