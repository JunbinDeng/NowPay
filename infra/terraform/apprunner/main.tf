variable "aws_account_id" {}
variable "aws_region" {
  default = "ap-southeast-2"
}
variable "ecr_repository" {
  default = "validator-service"
}

provider "aws" {
  region = var.aws_region
}

# IAM Role for App Runner to access ECR
resource "aws_iam_role" "app_runner_role" {
  name = "AppRunnerECRAccessRole"

  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
      Effect = "Allow"
      Principal = {
        Service = "apprunner.amazonaws.com"
      }
      Action = "sts:AssumeRole"
    }]
  })
}

resource "aws_iam_policy" "app_runner_ecr_policy" {
  name        = "AppRunnerECRPolicy"
  description = "Allow App Runner to pull images from ECR"

  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Action = [
          "ecr:GetDownloadUrlForLayer",
          "ecr:BatchGetImage",
          "ecr:DescribeImages",
          "ecr:GetAuthorizationToken"
        ],
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "app_runner_policy_attachment" {
  role       = aws_iam_role.app_runner_role.name
  policy_arn = aws_iam_policy.app_runner_ecr_policy.arn
}

# Deploy App Runner Service with ECR
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
      access_role_arn = aws_iam_role.app_runner_role.arn
    }

    auto_deployments_enabled = true
  }

  instance_configuration {
    cpu    = "256"  # 0.25 vCPU
    memory = "512"   # 0.5 GB RAM
  }
}

# Output the App Runner Service URL
output "app_runner_service_url" {
  value = aws_apprunner_service.my_service.service_url
}