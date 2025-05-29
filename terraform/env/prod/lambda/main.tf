module "lambda" {
  source                      = "../../../lambda"
  environment                 = "prod"
  db_username                 = var.db_username
  db_password                 = var.db_password
  cognito_user_pool_id        = var.cognito_user_pool_id
  cognito_user_pool_client_id = var.cognito_user_pool_client_id
  jwt_secret                  = var.jwt_secret
  aws_access_key_id           = var.aws_access_key_id
  aws_secret_access_key       = var.aws_secret_access_key
  aws_session_token           = var.aws_session_token
}