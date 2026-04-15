provider "aws" {
  region = var.region
}

data "archive_file" "lambda_package" {
  type        = "zip"
  source_dir  = var.package_source_directory
  output_path = "${path.module}/${var.function_name}.zip"
}

resource "aws_iam_role" "lambda_execution" {
  name = "${var.function_name}-execution-role"
  tags = var.tags

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "basic_execution" {
  role       = aws_iam_role.lambda_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_lambda_function" "benchmark" {
  function_name = var.function_name
  role          = aws_iam_role.lambda_execution.arn
  runtime       = "dotnet10"
  handler       = "BenchmarkApp.AwsLambdaHost"
  filename      = data.archive_file.lambda_package.output_path
  source_code_hash = data.archive_file.lambda_package.output_base64sha256
  publish          = true
  memory_size      = var.memory_size_mb
  timeout          = var.timeout_seconds
  architectures    = [var.architecture]
  tags             = var.tags
}

resource "aws_apigatewayv2_api" "benchmark" {
  name          = "${var.function_name}-http-api"
  protocol_type = "HTTP"
  tags          = var.tags
}

resource "aws_apigatewayv2_integration" "benchmark" {
  api_id                 = aws_apigatewayv2_api.benchmark.id
  integration_type       = "AWS_PROXY"
  integration_uri        = aws_lambda_function.benchmark.invoke_arn
  integration_method     = "POST"
  payload_format_version = "2.0"
}

resource "aws_apigatewayv2_route" "root" {
  api_id    = aws_apigatewayv2_api.benchmark.id
  route_key = "ANY /"
  target    = "integrations/${aws_apigatewayv2_integration.benchmark.id}"
}

resource "aws_apigatewayv2_route" "proxy" {
  api_id    = aws_apigatewayv2_api.benchmark.id
  route_key = "ANY /{proxy+}"
  target    = "integrations/${aws_apigatewayv2_integration.benchmark.id}"
}

resource "aws_apigatewayv2_stage" "benchmark" {
  api_id      = aws_apigatewayv2_api.benchmark.id
  name        = "$default"
  auto_deploy = true
  tags        = var.tags
}

resource "aws_lambda_permission" "allow_http_api" {
  statement_id  = "AllowHttpApiInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.benchmark.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.benchmark.execution_arn}/*/*"
}
