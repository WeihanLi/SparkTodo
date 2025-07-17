#!/usr/bin/env python3
"""
SparkTodo MCP Tools Python Example

This script demonstrates how to interact with the SparkTodo MCP tools using Python.
It provides a simple client for testing and integrating with the MCP API.
"""

import json
import requests
from typing import Dict, Any, Optional, List
from dataclasses import dataclass


@dataclass
class McpClient:
    """Simple client for SparkTodo MCP tools."""
    
    base_url: str
    token: Optional[str] = None
    
    def _headers(self) -> Dict[str, str]:
        """Get request headers with authentication."""
        headers = {"Content-Type": "application/json"}
        if self.token:
            headers["Authorization"] = f"Bearer {self.token}"
        return headers
    
    def get_tools(self) -> Dict[str, Any]:
        """Get available MCP tools."""
        response = requests.get(f"{self.base_url}/tools")
        response.raise_for_status()
        return response.json()
    
    def execute_tool(self, tool_name: str, arguments: Dict[str, Any]) -> Dict[str, Any]:
        """Execute a specific MCP tool."""
        response = requests.post(
            f"{self.base_url}/tools/{tool_name}",
            headers=self._headers(),
            json=arguments
        )
        response.raise_for_status()
        return response.json()
    
    # Todo operations
    def list_todos(self, page_index: int = 1, page_size: int = 50, 
                   only_not_done: bool = False, category_id: int = -1) -> Dict[str, Any]:
        """List todos with optional filtering."""
        return self.execute_tool("list_todos", {
            "pageIndex": page_index,
            "pageSize": page_size,
            "isOnlyNotDone": only_not_done,
            "categoryId": category_id
        })
    
    def get_todo(self, todo_id: int) -> Dict[str, Any]:
        """Get a specific todo by ID."""
        return self.execute_tool("get_todo", {"todoId": todo_id})
    
    def create_todo(self, title: str, content: str = "", category_id: int = 1,
                    scheduled_time: Optional[str] = None) -> Dict[str, Any]:
        """Create a new todo."""
        args = {
            "title": title,
            "content": content,
            "categoryId": category_id
        }
        if scheduled_time:
            args["scheduledTime"] = scheduled_time
        return self.execute_tool("create_todo", args)
    
    def update_todo(self, todo_id: int, title: Optional[str] = None,
                    content: Optional[str] = None, is_completed: Optional[bool] = None,
                    category_id: Optional[int] = None, 
                    scheduled_time: Optional[str] = None) -> Dict[str, Any]:
        """Update an existing todo."""
        args = {"todoId": todo_id}
        if title is not None:
            args["title"] = title
        if content is not None:
            args["content"] = content
        if is_completed is not None:
            args["isCompleted"] = is_completed
        if category_id is not None:
            args["categoryId"] = category_id
        if scheduled_time is not None:
            args["scheduledTime"] = scheduled_time
        return self.execute_tool("update_todo", args)
    
    def delete_todo(self, todo_id: int) -> Dict[str, Any]:
        """Delete a todo."""
        return self.execute_tool("delete_todo", {"todoId": todo_id})
    
    # Category operations
    def list_categories(self) -> Dict[str, Any]:
        """List all categories."""
        return self.execute_tool("list_categories", {})
    
    def create_category(self, name: str, parent_id: int = 0) -> Dict[str, Any]:
        """Create a new category."""
        return self.execute_tool("create_category", {
            "name": name,
            "parentId": parent_id
        })
    
    def update_category(self, category_id: int, name: Optional[str] = None,
                        parent_id: Optional[int] = None) -> Dict[str, Any]:
        """Update an existing category."""
        args = {"categoryId": category_id}
        if name is not None:
            args["name"] = name
        if parent_id is not None:
            args["parentId"] = parent_id
        return self.execute_tool("update_category", args)
    
    def delete_category(self, category_id: int) -> Dict[str, Any]:
        """Delete a category."""
        return self.execute_tool("delete_category", {"categoryId": category_id})


def demo():
    """Demonstrate MCP tools usage."""
    # Initialize client
    client = McpClient(
        base_url="https://localhost:5001/api/mcp",
        token="your_jwt_token_here"  # Replace with actual token
    )
    
    print("=== SparkTodo MCP Tools Python Demo ===\n")
    
    try:
        # 1. Get available tools
        print("1. Getting available MCP tools...")
        tools = client.get_tools()
        tool_names = [tool["name"] for tool in tools["tools"]]
        print(f"Available tools: {', '.join(tool_names)}\n")
        
        # 2. List categories
        print("2. Listing categories...")
        categories = client.list_categories()
        print(f"Categories: {json.dumps(categories, indent=2)}\n")
        
        # 3. Create a category
        print("3. Creating a new category...")
        new_category = client.create_category("Python Projects")
        print(f"Created category: {json.dumps(new_category, indent=2)}\n")
        
        # 4. Create a todo
        print("4. Creating a new todo...")
        new_todo = client.create_todo(
            title="Learn MCP integration",
            content="Study Model Context Protocol for AI assistant integration",
            category_id=1,
            scheduled_time="2024-02-01T10:00:00Z"
        )
        print(f"Created todo: {json.dumps(new_todo, indent=2)}\n")
        
        # 5. List todos
        print("5. Listing incomplete todos...")
        todos = client.list_todos(page_size=5, only_not_done=True)
        print(f"Todos: {json.dumps(todos, indent=2)}\n")
        
        # 6. Update todo (mark as completed)
        print("6. Marking todo as completed...")
        # Note: Replace todo_id=1 with actual ID from created todo
        updated_todo = client.update_todo(todo_id=1, is_completed=True)
        print(f"Updated todo: {json.dumps(updated_todo, indent=2)}\n")
        
        print("=== Demo Complete ===")
        
    except requests.exceptions.RequestException as e:
        print(f"Request error: {e}")
        print("Note: Make sure to replace 'your_jwt_token_here' with a valid JWT token")
        print("Note: Update the base_url to match your API endpoint")
    except Exception as e:
        print(f"Error: {e}")


if __name__ == "__main__":
    demo()