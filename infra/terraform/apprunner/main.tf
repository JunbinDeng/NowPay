variable "aws_account_id" {}
variable "aws_region" {
  default = "ap-southeast-2"
}
variable "ecr_repository" {
  default = "validator-service"
}

provider "aws" {
  region = "${var.aws_region}"
}

# --------------------------
# Use Existing IAM Role for App Runner to access ECR
# --------------------------
data "aws_iam_role" "existing_role" {
  name = "AppRunnerECRAccessRole"
}

data "aws_iam_policy" "existing_policy" {
  arn = "arn:aws:iam::${var.aws_account_id}:policy/AppRunnerECRPolicy"
}

# --------------------------
# Deploy App Runner Service with ECR
# --------------------------
resource "aws_apprunner_service" "my_service" {
  service_name = var.ecr_repository

  source_configuration {
    image_repository {
      image_identifier      = "${var.aws_account_id}.dkr.ecr.${var.aws_region}.amazonaws.com/${var.ecr_repository}:latest"
      image_repository_type = "ECR"

      image_configuration {
        port = "8080"
      }
    }

    authentication_configuration {
      access_role_arn = data.aws_iam_role.existing_role.arn
    }

    auto_deployments_enabled = true
  }

  instance_configuration {
    cpu    = "256"  # 0.25 vCPU
    memory = "512"   # 0.5 GB RAM
  }

  lifecycle {
    prevent_destroy = true
    ignore_changes = all
  }
}

# --------------------------
# Output the App Runner Service URL
# --------------------------
output "app_runner_service_url" {
  value = aws_apprunner_service.my_service.service_url
}