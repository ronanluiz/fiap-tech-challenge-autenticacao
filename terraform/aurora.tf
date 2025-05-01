# # aurora.tf - Recursos do Amazon Aurora RDS

# resource "aws_db_subnet_group" "aurora_subnet_group" {
#   name       = "${local.projeto}-aurora-subnet-group"
#   subnet_ids = flatten([module.vpc.private_subnets])

#   tags = {
#     Name = "${local.projeto}-aurora-subnet-group"
#   }
# }

# resource "aws_rds_cluster" "aurora" {
#   cluster_identifier      = "${local.projeto}-aurora-cluster"
#   engine                  = "aurora-postgresql"
#   database_name           = var.db_name
#   master_username         = var.db_username
#   master_password         = var.db_password
#   backup_retention_period = 5
#   preferred_backup_window = "07:00-09:00"
#   skip_final_snapshot     = true
#   vpc_security_group_ids  = [aws_security_group.aurora_sg.id]
#   db_subnet_group_name    = aws_db_subnet_group.aurora_subnet_group.name

#   tags = {
#     Name = "${local.projeto}-aurora-cluster"
#   }
# }

# resource "aws_rds_cluster_instance" "aurora_instances" {
#   count                = 1
#   identifier           = "${local.projeto}-aurora-${count.index}"
#   cluster_identifier   = aws_rds_cluster.aurora.id
#   instance_class       = "db.t3.medium"
#   engine               = "postgres"
#   db_subnet_group_name = aws_db_subnet_group.aurora_subnet_group.name

#   tags = {
#     Name = "${local.projeto}-aurora-${count.index}"
#   }
# }