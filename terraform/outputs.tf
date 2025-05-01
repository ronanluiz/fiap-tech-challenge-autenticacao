# outputs.tf

output "api_gateway_url" {
  value = "${aws_api_gateway_stage.stage.invoke_url}/auth"
}

# output "aurora_endpoint" {
#   value = aws_rds_cluster.aurora.endpoint
# }

output "cognito_user_pool_id" {
  value = aws_cognito_user_pool.user_pool.id
}

output "cognito_client_id" {
  value = aws_cognito_user_pool_client.client.id
}