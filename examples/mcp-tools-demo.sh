#!/bin/bash

# SparkTodo MCP Tools Usage Examples
# This script demonstrates how to interact with the MCP tools

# Configuration
API_BASE="https://localhost:5001/api/mcp"
TOKEN="your_jwt_token_here"

echo "=== SparkTodo MCP Tools Demo ==="
echo ""

# 1. Get available tools
echo "1. Getting available MCP tools..."
curl -X GET "${API_BASE}/tools" \
  -H "Content-Type: application/json" \
  | jq '.tools[].name' 2>/dev/null || echo "Response received (install jq for formatted output)"

echo -e "\n"

# 2. List categories
echo "2. Listing categories..."
curl -X POST "${API_BASE}/tools/list_categories" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{}'

echo -e "\n"

# 3. Create a category
echo "3. Creating a new category..."
curl -X POST "${API_BASE}/tools/create_category" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Work Projects",
    "parentId": 0
  }'

echo -e "\n"

# 4. Create a todo
echo "4. Creating a new todo..."
curl -X POST "${API_BASE}/tools/create_todo" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Implement MCP tools",
    "content": "Create Model Context Protocol tools for the todo API",
    "categoryId": 1,
    "scheduledTime": "2024-02-01T09:00:00Z"
  }'

echo -e "\n"

# 5. List todos
echo "5. Listing todos..."
curl -X POST "${API_BASE}/tools/list_todos" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "pageSize": 5,
    "isOnlyNotDone": true
  }'

echo -e "\n"

# 6. Update todo (mark as completed)
echo "6. Marking todo as completed..."
curl -X POST "${API_BASE}/tools/update_todo" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "todoId": 1,
    "isCompleted": true
  }'

echo -e "\n"

# 7. Get specific todo
echo "7. Getting specific todo..."
curl -X POST "${API_BASE}/tools/get_todo" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "todoId": 1
  }'

echo -e "\n\n=== Demo Complete ==="
echo "Note: Replace 'your_jwt_token_here' with a valid JWT token"
echo "Note: Update todoId and categoryId values based on your data"