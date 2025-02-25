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

- HTTP: `http://localhost:5011`
- HTTPS: `https://localhost:5012`

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
- Docker: `https://localhost:5012/api/validator`

### Swagger UI

Swagger is available at:

```
https://localhost:7002/swagger/index.html
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
    "message": "Valid card number."
  }
  ```

- **400 Bad Request (Missing Field)**

  ```json
  {
    "status": 400,
    "message": "The 'cardNumber' field is required.",
    "error": {
      "code": "missing_field",
      "details": "Please provide a valid card number."
    }
  }
  ```

- **422 Unprocessable Entity (Invalid Card Number Length.)**

  ```json
  {
    "status": 422,
    "message": "Invalid card number length.",
    "error": {
      "code": "invalid_length",
      "details": "Card number length must be between 13 and 19 digits."
    }
  }
  ```

- **422 Unprocessable Entity (Invalid Card Number Format)**

  ```json
  {
    "status": 422,
    "message": "Invalid card number format.",
    "error": {
      "code": "invalid_format",
      "details": "Card number must contain only numeric digits (0-9)."
    }
  }
  ```

- **422 Unprocessable Entity (Invalid Card Number Format)**

  ```json
  {
    "status": 422,
    "message": "Invalid card number.",
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
    "message": "An error occurred while processing your request.",
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

### 2. Deploy to ECS using Terraform (WIP)

```shell
cd infra/terraform
terraform apply
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m "Add new feature"`)
4. Push the branch (`git push origin feature/your-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.
