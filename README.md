# Purple Explorer - cross-platform Azure Service Bus explorer (Windows, macOS, Linux)

Purple Explorer is a cross-platform desktop application built with .NET Core. 

It's a simple tool to help you: 
* Connect to Azure Service Bus
* View topics and subscriptions
* View active and dead-letter messages
* View message body and details
* Send a new message
* Save messages to send them quickly
* Delete a message^
* Purge active or dead-letter messages
* Re-submit a message from dead-letter
* Dead-letter a message^

\^ NOTE: These actions require receiving all the messages up to the selected message and this increases DeliveryCount. Be aware that there can be consequences to other messages in the subscription.

## How to Download
Win-x64, macOS-x64 and linux-x64 pre-built binaries can be found on [Releases](https://github.com/telstrapurple/PurpleExplorer/releases) page

## How to run
### Windows
> _Right-click_ -> Open

You can allow windows defender to start this application:
> click on _More Info_ -> Run anyway

### macOS
> _Right-click_ -> Open

You can allow macOS to start this application by enabling Developer tools for Terminal:
> _System Preferences -> Security & Privacy -> Privacy_, select "Developer Tools" on the left, check terminal on the right.

You can make `PurpleExplorer` file executable by:
> chmod +x PurpleExplorer

### Linux (CentOS, Debian, Fedora, Ubuntu and derivatives)
> _Right-click_ -> Run