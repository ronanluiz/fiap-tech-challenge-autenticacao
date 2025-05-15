output "cognito_user_pool_client_id" {
  value = length(aws_cognito_user_pool_client.client) > 0 ? aws_cognito_user_pool_client.client[0].id : ""
}

output "cognito_user_pool_id" {
  value = length(aws_cognito_user_pool.user_pool) > 0 ? aws_cognito_user_pool.user_pool[0].id : ""
}