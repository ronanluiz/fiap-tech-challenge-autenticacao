# network.tf - Recursos de rede (VPC, Subnets, etc.)
data "aws_vpc" "vpc" {
  tags = {
    Name = "${var.ambiente}-tc-soat10-vpc"
  }
}

data "aws_subnets" "subnets" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.vpc.id]
  }
}

data "aws_subnets" "private" {
  filter {
    name   = "tag:Name"
    values = ["*private*"] # ou qualquer padrão usado para nomear suas subnets privadas
  }

  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.vpc.id]
  }
}

data "aws_security_group" "rds" {
  name = "${var.ambiente}-tc-bd-sg"
}

resource "aws_security_group" "lambda" {
  name        = "${local.projeto}-lambda-sg"
  description = "Security group for Lambda function"
  vpc_id      = data.aws_vpc.vpc.id

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${local.projeto}-lambda-sg"
  }
}

resource "aws_security_group" "rds_sg" {
  name        = "${local.projeto}-rds-lambda-sg"
  description = "Permite acesso ao RDS a partir da função Lambda"
  vpc_id      = data.aws_vpc.vpc.id

  # Regra de entrada que permite conexão à porta do banco de dados (ex: MySQL = 3306)
  ingress {
    description     = "Acesso do Lambda ao RDS"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.lambda.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${local.projeto}-rds-lambda-sg"
  }
}