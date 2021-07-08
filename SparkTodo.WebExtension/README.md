# SparkTodo.WebExtension

## Intro

这是一个简单的基于 Blazor 开发的浏览器扩展，尝试一下 C# 不一样的玩法，体验一下使用 C# 写浏览器插件的快乐~~

## How does it works

基于 Blazor WebAssembly 运行的浏览器扩展，主要是 `Blazor.BrowserExtension` 来实现的，`Blazor.BrowserExtension`依赖的 `WebExtensions.Net` 将浏览器的扩展 API 翻译成了 .NET API 使得我们可以强类型的和浏览器扩展进行交互，而 `Blazor.BrowserExtension` 在其基础之上又借助 MS Build Task 来自动化地帮助我们生成浏览器扩展所需的必要文件从而大大简化我们开发的难度，极其容易上手

如果你对 C# 开发浏览器扩展感兴趣，这一定是你的不二之选。

## Features

目前只是数据都在本地的一个浏览器扩展，主要是做了一个 push notification。

## References

- [Official Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/?WT.mc_id=DT-MVP-5004222)
- [MS Learn for Blazor Tutorial](https://docs.microsoft.com/en-us/learn/modules/build-blazor-webassembly-visual-studio-code/?WT.mc_id=DT-MVP-5004222)
- [Blazor Extensions for build a browser extensions](https://github.com/mingyaulee/Blazor.BrowserExtension)
- <https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions>
- <https://developer.chrome.com/docs/extensions/mv3/getstarted/>
- <https://github.com/GoogleChrome/chrome-extensions-samples>

