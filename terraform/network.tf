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

data "aws_security_group" "lambda" {
  name = "${var.ambiente}-tc-soat10-lambda-sg"
}

# resource "aws_db_subnet_group" "postgres_subnet_group" {
#   name       = "${local.projeto}-subnet-group"
#   subnet_ids = data.aws_subnets.subnets.ids

#   tags = {
#     Name = "Postgres Subnet Group"
#   }
# }


# Grupos de segurança
# resource "aws_security_group" "aurora_sg" {
#   name        = "${local.projeto}-aurora-sg"
#   description = "Security group for Aurora RDS"
#   vpc_id      = data.aws_vpc.vpc.id

#   ingress {
#     from_port       = 5432
#     to_port         = 5432
#     protocol        = "tcp"
#     security_groups = [aws_security_group.lambda_sg.id]
#     description     = "Enable access from lambda function"
#   }

#   egress {
#     from_port   = 0
#     to_port     = 0
#     protocol    = "-1"
#     cidr_blocks = ["0.0.0.0/0"]
#   }

#   tags = {
#     Name = "${local.projeto}-aurora-sg"
#   }
# }

# resource "aws_security_group" "lambda_sg" {
#   name        = "${local.projeto}-lambda-sg"
#   description = "Security group for Lambda function"
#   vpc_id      = module.vpc.vpc_id

#   egress {
#     from_port   = 0
#     to_port     = 0
#     protocol    = "-1"
#     cidr_blocks = ["0.0.0.0/0"]
#   }

#   tags = {
#     Name = "${local.projeto}-lambda-sg"
#   }
# }