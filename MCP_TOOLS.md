# MCP Tools for SparkTodo API

This implementation exposes the SparkTodo API functionality as Model Context Protocol (MCP) tools, allowing AI assistants to interact with the todo system in a standardized way.

## Overview

The MCP implementation provides a `/api/mcp` endpoint that exposes todo and category management functionality as standardized tools that can be consumed by MCP-compatible AI assistants.

## Available Tools

### Todo Management Tools

1. **list_todos** - List todo items with optional filtering
   - Parameters: `pageIndex`, `pageSize`, `isOnlyNotDone`, `categoryId`
   - Returns: Paginated list of todos

2. **get_todo** - Get a specific todo item by ID
   - Parameters: `todoId` (required)
   - Returns: Todo item details

3. **create_todo** - Create a new todo item
   - Parameters: `title` (required), `content`, `categoryId` (required), `scheduledTime`
   - Returns: Created todo item

4. **update_todo** - Update an existing todo item
   - Parameters: `todoId` (required), `title`, `content`, `isCompleted`, `categoryId`, `scheduledTime`
   - Returns: Updated todo item

5. **delete_todo** - Delete a todo item
   - Parameters: `todoId` (required)
   - Returns: Success message

### Category Management Tools

6. **list_categories** - List all categories for the user
   - Parameters: None
   - Returns: List of categories

7. **create_category** - Create a new category
   - Parameters: `name` (required), `parentId`
   - Returns: Created category

8. **update_category** - Update an existing category
   - Parameters: `categoryId` (required), `name`, `parentId`
   - Returns: Updated category

9. **delete_category** - Delete a category
   - Parameters: `categoryId` (required)
   - Returns: Success message

## API Endpoints

### GET /api/mcp/tools
Returns the list of available tools with their schemas.

**Example Response:**
```json
{
  "tools": [
    {
      "name": "list_todos",
      "description": "List todo items for the authenticated user with optional filtering",
      "inputSchema": {
        "type": "object",
        "properties": {
          "pageIndex": { "type": "integer", "description": "Page index (default: 1)", "minimum": 1 },
          "pageSize": { "type": "integer", "description": "Page size (default: 50)", "minimum": 1, "maximum": 100 },
          "isOnlyNotDone": { "type": "boolean", "description": "Filter only incomplete todos (default: false)" },
          "categoryId": { "type": "integer", "description": "Filter by category ID (default: -1 for all)" }
        }
      }
    }
  ]
}
```

### POST /api/mcp/tools/{toolName}
Executes a specific tool with the provided arguments.

**Example Request (create_todo):**
```json
{
  "title": "Complete project documentation",
  "content": "Write comprehensive documentation for the MCP tools implementation",
  "categoryId": 1,
  "scheduledTime": "2024-01-15T14:30:00Z"
}
```

**Example Response:**
```json
{
  "success": true,
  "result": {
    "todoId": 123,
    "todoTitle": "Complete project documentation",
    "todoContent": "Write comprehensive documentation for the MCP tools implementation",
    "categoryId": 1,
    "userId": 1,
    "isCompleted": false,
    "createdTime": "2024-01-14T10:00:00Z",
    "updatedTime": "2024-01-14T10:00:00Z",
    "scheduledTime": "2024-01-15T14:30:00Z"
  }
}
```

## Authentication

All tool execution endpoints require authentication using the existing JWT authentication mechanism. The tools automatically filter data based on the authenticated user.

## Usage Examples

### Get Available Tools
```bash
curl -X GET https://api.example.com/api/mcp/tools
```

### List Todos
```bash
curl -X POST https://api.example.com/api/mcp/tools/list_todos \
  -H "Authorization: Bearer {jwt_token}" \
  -H "Content-Type: application/json" \
  -d '{"pageSize": 10, "isOnlyNotDone": true}'
```

### Create Todo
```bash
curl -X POST https://api.example.com/api/mcp/tools/create_todo \
  -H "Authorization: Bearer {jwt_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "New task",
    "content": "Task description", 
    "categoryId": 1
  }'
```

### Update Todo
```bash
curl -X POST https://api.example.com/api/mcp/tools/update_todo \
  -H "Authorization: Bearer {jwt_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "todoId": 123,
    "isCompleted": true
  }'
```

### List Categories
```bash
curl -X POST https://api.example.com/api/mcp/tools/list_categories \
  -H "Authorization: Bearer {jwt_token}" \
  -H "Content-Type: application/json" \
  -d '{}'
```

## Error Handling

When a tool execution fails, the API returns an error response:

```json
{
  "success": false,
  "error": "Todo item not found"
}
```

Common error scenarios:
- Missing required parameters
- Invalid parameter values
- Resource not found
- Unauthorized access
- Database operation failures

## Implementation Notes

- All tools respect user isolation - users can only access their own todos and categories
- The implementation reuses existing repository patterns and business logic
- Error handling provides clear error messages for invalid requests
- All date/time values are handled in UTC
- Soft deletion is respected (deleted items are not returned)
- The MCP controller follows the same patterns as other API controllers in the project

## Integration

MCP-compatible AI assistants can discover and use these tools by:
1. Calling the `/api/mcp/tools` endpoint to get available tools
2. Using the returned schema information to understand tool parameters
3. Executing tools via the `/api/mcp/tools/{toolName}` endpoint

## Schema Validation

A complete JSON schema for all MCP tools is available in `mcp-tools-schema.json` for validation and documentation purposes.