# Purple Explorer - cross-platform Azure Service Bus explorer (Windows, macOS, Linux)

Purple Explorer is a cross-platform desktop application built with .NET Core. 

It's a simple tool to help you to: 
* connect to an Azure service bus instance
* list topics and subscriptions
* see active and dead-letter queue messages
* send a new message
* delete a message
* re-submit a message from DLQ
* purge all messages

## How to Download
Win-x64, macOS-x64 and linux-x64 pre-built binaries can be found on [Actions](https://github.com/telstrapurple/PurpleExplorer/actions?query=workflow%3A%22.NET+Core%22+branch%3Amaster) page

## How to run
### Windows
> _Right-click_ -> Open

You can allow windows defender to start this application:
> click on _More Info_ -> Run anyway

### macOS
> chmod +x PurpleExplorer

> _Right-click_ -> Open

You can allow macOS to start this application by enabling Developer tools for Terminal:
> _System Preferences -> Security & Privacy -> Privacy_, select "Developer Tools" on the left, check terminal on the right.
