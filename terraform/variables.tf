# variables.tf
locals {
  projeto = "${var.ambiente}-tc-autenticacao"
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

variable "bd_nome" {
  type = string
}

variable "bd_usuario" {
  type = string
}

variable "bd_senha" {
  type = string
}

variable "bd_host" {
  type        = string
  sensitive   = true
}

variable "cognito_user_pool_id" {
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
