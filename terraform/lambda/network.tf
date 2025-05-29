# network.tf - Recursos de rede (VPC, Subnets, etc.)
data "aws_vpc" "vpc" {
  tags = {
    Name = local.vpc_name
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

resource "aws_security_group" "lambda" {
  name        = "${local.project}-lambda-sg"
  description = "Security group for Lambda function"
  vpc_id      = data.aws_vpc.vpc.id

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${local.project}-lambda-sg"
  }
}

resource "aws_security_group" "rds" {
  name        = "${local.project}-rds-lambda-sg"
  description = "Libera acesso ao RDS a partir da Lambda"
  vpc_id      = data.aws_vpc.vpc.id
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
    Name = "${local.project}-rds-lambda-sg"
  }
}