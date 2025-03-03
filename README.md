# SparkTodo

## Intro

SparkTodo 是一个基于 ASP.NET Core 的一个 TodoList WebApi 项目，使用 swagger 自动生成接口文档，身份验证使用 JsonWebToken 来验证。

项目演示地址： <https://sparktodo.weihanli.xyz/>

> 注：该地址仅供演示，请勿正式使用，造成的数据丢失概不负责

## Setup

- dotnet sdk: <https://get.dot.net>
- aspire dashabord: <https://aspiredashboard.com/>

run aspire-dashboard container with docker:

```sh
docker run -p 18888:18888 -p 4317:18889 -d --name aspire-dashboard -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS="true" mcr.microsoft.com/dotnet/aspire-dashboard:9.1
```
