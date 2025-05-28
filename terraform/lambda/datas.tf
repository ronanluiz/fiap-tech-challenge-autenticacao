data "aws_iam_role" "lab_role" {
  name = "LabRole"
}

data "aws_db_instance" "database" {
  db_instance_identifier = local.db_instance_identifier
}