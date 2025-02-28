# NowPay - Validator Service

## Overview

The **Validator Service** is a microservice within the **NowPay** ecosystem that provides credit card validation using the Luhn algorithm. This service is designed to ensure the correctness of credit card numbers before processing transactions.

## Features

- Validates credit card numbers using the **Luhn algorithm**.
- Supports both **HTTP and HTTPS** requests.
- Dockerized for **local development** and **AWS ECS deployment**.
- Provides structured API responses with **proper error handling**.
- **Swagger UI** integration for API documentation and testing.

## Project Structure

```
NowPay/
│── src/
│   ├── ValidatorService/  # The Validator Service API
│── tests/
│   ├── ValidatorService.UnitTests/  # Unit tests
│   ├── ValidatorService.IntegrationTests/  # Integration tests
│── infra/
│   ├── devcerts/  # SSL Certificates for local development
│── docker-compose.yml
│── README.md
```

## Prerequisites

### Development Environment

- **.NET 9 SDK**
- **Docker & Docker Compose**

### HTTPS Certificate Setup (Required for Local HTTPS)

**Option 1: Use Shell Script (Recommended)**

Run the following script to generate and trust the required HTTPS certificates and start the service.

```shell
chmod +x prepare_project.sh

./prepare_project.sh
```

**Option 2: Manual Setup**

If you prefer manual setup, follow these steps:

```shell
# Generate a self-signed development certificate
mkdir -p infra/devcerts

dotnet dev-certs https -ep infra/devcerts/aspnetapp.pfx -p aspnetapp

dotnet dev-certs https --trust  # Trust the certificate on your local machine
```

## Running the Service

### 1. Running Locally (without Docker)

```shell
cd src/ValidatorService

dotnet run
```

The API should be available at:

- HTTP: `http://localhost:7001`
- HTTPS: `https://localhost:7002`

### 2. Running with Docker

```shell
docker-compose up --build
```

The API should be accessible at:

- HTTP: `http://localhost:5001`
- HTTPS: `https://localhost:5002`

### 3. Running Unit & Integration Tests

```shell
cd tests/ValidatorService.UnitTests

dotnet test --logger "trx;LogFileName=test-results.trx"

cd tests/ValidatorService.IntegrationTests

dotnet test --logger "trx;LogFileName=test-results.trx"
```

## API Documentation

### Base URL

- Local: `https://localhost:7002/api/validator`
- Docker: `https://localhost:5002/api/validator`

### Swagger UI

Swagger is available at:

```
- Local: `https://localhost:7002`
- Docker: `https://localhost:5002`
```

### API Endpoints

#### 1. Validate Credit Card (Luhn Algorithm)

**Endpoint:** `POST /api/validator/luhn`

**Request Body:**

```json
{
  "cardNumber": "4111111111111111"
}
```

**Response Examples:**

- **200 OK (Valid Card Number)**

  ```json
  {
    "status": 200,
    "data": {
      "isValid": true
    }
  }
  ```

- **400 Bad Request (Missing Field)**

  ```json
  {
    "status": 400,
    "error": {
      "code": "missing_field",
      "details": "Please provide a valid card number."
    }
  }
  ```

- **200 OK (Invalid Card Number Length)**

  ```json
  {
    "status": 200,
    "data": {
      "isValid": false
    },
    "error": {
      "code": "invalid_length",
      "details": "Card number length must be between 13 and 19 digits."
    }
  }
  ```

- **200 OK (Invalid Card Number Format)**

  ```json
  {
    "status": 200,
    "data": {
      "isValid": false
    },
    "error": {
      "code": "invalid_format",
      "details": "Card number must contain only numeric digits (0-9)."
    }
  }
  ```

- **200 OK (Invalid Card Number)**

  ```json
  {
    "status": 200,
    "data": {
      "isValid": false
    },
    "error": {
      "code": "invalid_number",
      "details": "The card number does not pass the Luhn algorithm validation, please try again."
    }
  }
  ```

- **500 Internal Server Error**

  ```json
  {
    "status": 500,
    "error": {
      "code": "internal_error",
      "details": "An unexpected error occurred. Please try again later."
    }
  }
  ```

## Deployment (AWS ECS)

The Validator Service can be deployed to AWS ECS using Fargate.

### 1. Build & Push Docker Image to AWS ECR

```shell
aws ecr get-login-password --region <your-region> | docker login --username AWS --password-stdin <your-account-id>.dkr.ecr.<your-region>.amazonaws.com

docker buildx build --platform linux/amd64 -t validator-service .

docker tag validator-service <your-account-id>.dkr.ecr.<your-region>.amazonaws.com/validator-service:latest

docker push <your-account-id>.dkr.ecr.<your-region>.amazonaws.com/validator-service:latest
```

### 2. Deploy to ECS using Terraform

#### 1. Navigate to the Terraform Directory

```
cd infra/terraform
```

#### 2. Create or Edit `terraform.tfvars`

Terraform uses `terraform.tfvars` for variable definitions. Create or update this file with the required AWS settings:

```
# infra/terraform/terraform.tfvars

# Required AWS Account ID
aws_account_id = "123456789012"

# Optional Variables (adjust as needed)
aws_region     = "us-east-1"
ecr_repository = "my-aspnet-service"
```

**Note**: Ensure `terraform.tfvars` is **not** committed to version control (add it to `.gitignore`).

#### 3. Deployment

Run the following command to initialize Terraform (only required on the first run or after updating providers):

```
# Initialize Terraform
terraform init

# Review the Terraform Plan(Optional)
terraform plan

# Apply Terraform Configuration
terraform apply
```

## Troubleshooting

**Potential CORS Issues When Using HTTP**

If you access this API over **HTTP** (for example, http://localhost:5001) from a site or page served over **HTTPS**, you might encounter “Failed to fetch” or **CORS** (Cross-Origin Resource Sharing) errors. Modern browsers often block mixed-content requests (HTTPS page calling HTTP endpoint), or you may need additional server-side CORS headers. This is **normal** behavior when using HTTP in a secure context.

**How to Fix or Avoid Mixed Content**:

• Use **HTTPS** consistently for both your site and the API.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m "Add new feature"`)
4. Push the branch (`git push origin feature/your-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.
