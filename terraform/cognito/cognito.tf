# cognito.tf - Recursos do Amazon Cognito
resource "aws_cognito_user_pool" "user_pool" {
  name = "${local.project}-user-pool"

  # Configurações de atributos
  schema {
    name                = "name"
    attribute_data_type = "String"
    required            = true
    mutable             = true
  }

  schema {
    name                = "email"
    attribute_data_type = "String"
    required            = false
    mutable             = true
  }

  # Atributo personalizado para CPF
  schema {
    name                     = "cpf"
    attribute_data_type      = "String"
    required                 = false
    mutable                  = false
    developer_only_attribute = false
  }

  admin_create_user_config {
    allow_admin_create_user_only = true
  }

  password_policy {
    minimum_length                   = 8
    require_lowercase                = false
    require_numbers                  = false
    require_symbols                  = false
    require_uppercase                = false
    temporary_password_validity_days = 7
  }

  tags = {
    Name = "${local.project}-user-pool"
  }
}

resource "aws_cognito_user_pool_client" "client" {
  name         = "${local.project}-user-pool-client"
  user_pool_id = aws_cognito_user_pool.user_pool.id

  explicit_auth_flows = [
    "ADMIN_NO_SRP_AUTH",
    "USER_PASSWORD_AUTH"
  ]

  generate_secret = false
}