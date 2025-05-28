# variables.tf
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
