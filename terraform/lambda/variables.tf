# variables.tf
locals {
  project                = "${var.environment}-${var.project_name}"
  vpc_name               = "${var.environment}-vpc"
  db_instance_identifier = "${var.environment}-tc-bd"
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

variable "db_username" {
  type = string
}

variable "db_password" {
  type = string
}

variable "cognito_user_pool_id" {
  description = "Id do User Pool Id do Congnito"
  type        = string
}

variable "cognito_user_pool_client_id" {
  description = "Id do User Pool Id do Congnito"
  type        = string
}

variable "jwt_secret" {
  description = "Chave secreta para assinar tokens JWT"
  type        = string
  sensitive   = true
}

variable "lambda_zip_path" {
  description = "Caminho para o arquivo ZIP contendo o código da função Lambda"
  type        = string
  default     = "./lambda_function.zip"
}

variable "aws_access_key_id" {
  type      = string
  sensitive = true
}

variable "aws_secret_access_key" {
  type      = string
  sensitive = true
}
variable "aws_session_token" {
  type      = string
  sensitive = true
}

variable "project_name" {
  type    = string
  default = "tc-autenticacao"
}
