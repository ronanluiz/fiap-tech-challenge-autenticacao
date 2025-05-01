# cognito.tf - Recursos do Amazon Cognito
resource "aws_cognito_user_pool_client" "client" {
  name         = "${local.projeto}-app-client"
  user_pool_id = var.cognito_user_pool_id

  explicit_auth_flows = [
    "ADMIN_NO_SRP_AUTH",
    "USER_PASSWORD_AUTH"
  ]

  generate_secret = false
}