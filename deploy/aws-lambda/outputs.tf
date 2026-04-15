output "api_base_url" {
  description = "Public base URL for the benchmark app."
  value       = aws_apigatewayv2_stage.benchmark.invoke_url
}

output "function_name" {
  description = "Lambda function name."
  value       = aws_lambda_function.benchmark.function_name
}
