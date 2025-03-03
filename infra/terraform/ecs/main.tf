variable "aws_account_id" {}
variable "aws_region" {
  default = "ap-southeast-2"
}
variable "ecr_repository" {
  default = "validator-service"
}

# Use existing IAM role instead of creating a new one
data "aws_iam_role" "ecs_task_execution_role" {
  name = "ecsTaskExecutionRole"
}

provider "aws" {
  region = "${var.aws_region}"
}

# --------------------------
# VPC & Networking
# --------------------------
resource "aws_vpc" "ecs_vpc" {
  cidr_block = "10.0.0.0/16"
}

resource "aws_subnet" "ecs_subnet_1" {
  vpc_id            = aws_vpc.ecs_vpc.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = "${var.aws_region}a" # Change this to your preferred az
  map_public_ip_on_launch = true
}

resource "aws_subnet" "ecs_subnet_2" {
  vpc_id            = aws_vpc.ecs_vpc.id
  cidr_block        = "10.0.2.0/24"
  availability_zone = "${var.aws_region}b" # Change this to your preferred az
  map_public_ip_on_launch = true
}

resource "aws_security_group" "ecs_sg" {
  vpc_id = aws_vpc.ecs_vpc.id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 8080
    to_port     = 8080
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# --------------------------
# Internet Gateway
# --------------------------
resource "aws_internet_gateway" "ecs_igw" {
  vpc_id = aws_vpc.ecs_vpc.id
}

# --------------------------
# Route Table for Public Subnets
# --------------------------
resource "aws_route_table" "ecs_route_table" {
  vpc_id = aws_vpc.ecs_vpc.id
}

# --------------------------
# Add Default Route to Internet Gateway
# --------------------------
resource "aws_route" "ecs_internet_access" {
  route_table_id         = aws_route_table.ecs_route_table.id
  destination_cidr_block = "0.0.0.0/0"
  gateway_id             = aws_internet_gateway.ecs_igw.id
}

# --------------------------
# Associate Route Table with Public Subnets
# --------------------------
resource "aws_route_table_association" "ecs_subnet_1" {
  subnet_id      = aws_subnet.ecs_subnet_1.id
  route_table_id = aws_route_table.ecs_route_table.id
}

resource "aws_route_table_association" "ecs_subnet_2" {
  subnet_id      = aws_subnet.ecs_subnet_2.id
  route_table_id = aws_route_table.ecs_route_table.id
}

# --------------------------
# ECS Cluster
# --------------------------
resource "aws_ecs_cluster" "validator_cluster" {
  name = "validator-cluster"
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution_policy" {
  role       = data.aws_iam_role.ecs_task_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# --------------------------
# ECS Task Definition
# --------------------------
resource "aws_ecs_task_definition" "validator_task" {
  family                   = "validator-task"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = "256"
  memory                   = "512"
  execution_role_arn       = data.aws_iam_role.ecs_task_execution_role.arn

  container_definitions = jsonencode([
    {
      name      = "${var.ecr_repository}"
      image     = "${var.aws_account_id}.dkr.ecr.${var.aws_region}.amazonaws.com/${var.ecr_repository}:latest"
      cpu       = 256
      memory    = 512
      essential = true

      portMappings = [
        {
          containerPort = 8080
          hostPort      = 8080
        }
      ]

      environment = [
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:8080"
        }
      ]
    }
  ])
}

# --------------------------
# ECS Service
# --------------------------
resource "aws_ecs_service" "validator_service" {
  name            = "${var.ecr_repository}"
  cluster         = aws_ecs_cluster.validator_cluster.id
  task_definition = aws_ecs_task_definition.validator_task.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = [aws_subnet.ecs_subnet_1.id, aws_subnet.ecs_subnet_2.id]
    security_groups  = [aws_security_group.ecs_sg.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.validator_tg.arn
    container_name   = "${var.ecr_repository}"
    container_port   = 8080
  }
}

# --------------------------
# Auto Scaling Target
# --------------------------
resource "aws_appautoscaling_target" "ecs_target" {
  max_capacity       = 1  # Allow scaling up to 1 task
  min_capacity       = 0  # Scale down to zero when not in use
  resource_id        = "service/${aws_ecs_cluster.validator_cluster.name}/${aws_ecs_service.validator_service.name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

# --------------------------------
# Auto Scaling Policy - Scale Out
# --------------------------------
resource "aws_appautoscaling_policy" "scale_out" {
  name               = "validator-scale-out"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.ecs_target.resource_id
  scalable_dimension = aws_appautoscaling_target.ecs_target.scalable_dimension
  service_namespace  = aws_appautoscaling_target.ecs_target.service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }
    target_value = 50.0  # Scale up when CPU > 50%
  }
}

# --------------------------------
# Auto Scaling Policy - Scale In
# --------------------------------
resource "aws_appautoscaling_policy" "scale_in" {
  name               = "validator-scale-in"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.ecs_target.resource_id
  scalable_dimension = aws_appautoscaling_target.ecs_target.scalable_dimension
  service_namespace  = aws_appautoscaling_target.ecs_target.service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }
    target_value = 20.0  # Scale down when CPU < 20%
  }
}

# --------------------------
# Load Balancer
# --------------------------
resource "aws_lb" "validator_lb" {
  name               = "validator-lb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.ecs_sg.id]
  subnets           = [aws_subnet.ecs_subnet_1.id, aws_subnet.ecs_subnet_2.id]
}

resource "aws_lb_target_group" "validator_tg" {
  name     = "validator-tg"
  port     = 8080
  protocol = "HTTP"
  vpc_id   = aws_vpc.ecs_vpc.id
  target_type = "ip"

  health_check {
    path                = "/healthz"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 3
  }
}

resource "aws_lb_listener" "validator_listener" {
  load_balancer_arn = aws_lb.validator_lb.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.validator_tg.arn
  }
}

# --------------------------
# Output Load Balancer URL
# --------------------------
output "load_balancer_url" {
  value = aws_lb.validator_lb.dns_name
}