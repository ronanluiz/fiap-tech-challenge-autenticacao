resource "aws_lambda_function" "autenticacao_lambda" {
  function_name = "${local.projeto}-function"
  role          = data.aws_iam_role.lab_role.arn
  handler       = "AutenticacaoFunction::AutenticacaoFunction.Function::FunctionHandler"
  runtime       = "dotnet8"
  timeout       = 30
  memory_size   = 512

  filename         = var.lambda_zip_path
  source_code_hash = filebase64sha256(var.lambda_zip_path)

  environment {
    variables = {
      BD_HOST                       = var.bd_host
      BD_NOME                       = var.bd_nome
      BD_USUARIO                    = var.bd_usuario
      BD_SENHA                      = var.bd_senha
      COGNITO_USER_POOL_ID          = var.cognito_user_pool_id
      COGNITO_CLIENT_ID             = aws_cognito_user_pool_client.client.id
      JWT_SECRET                    = var.jwt_secret
      AWS_ACCESS_KEY_ID_COGNITO     = var.aws_access_key_id
      AWS_SECRET_ACCESS_KEY_COGNITO = var.aws_secret_access_key
      AWS_SESSION_TOKEN_COGNITO     = var.aws_session_token

    }
  }

  vpc_config {
    subnet_ids         = data.aws_subnets.subnets.ids
    security_group_ids = [data.aws_security_group.lambda.id]
  }

  tags = {
    Name = "${local.projeto}-lambda"
  }
}

# Permissão para que o API Gateway invoque a função Lambda
resource "aws_lambda_permission" "apigw_lambda" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.autenticacao_lambda.function_name
  principal     = "apigateway.amazonaws.com"

  # O ARN do API Gateway com wildcard /*
  source_arn = "${aws_api_gateway_rest_api.api.execution_arn}/*/*"
}