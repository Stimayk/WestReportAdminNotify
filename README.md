# WestReportAdminNotify
Modular report system for your CS:2 server - WestReportSystem

**Notify admins about a report in chat/hud**

The WRAN module itself is also customized
Configuration file:
```
{
  "AdminNotifyAdmFlag": "@css/ban", // Admin flags that allow you to receive notifications, default - @css/ban
  "AdminNotifyChat": true, // Send notification to chat (true - send, false - do not send), default - send
  "AdminNotifyHUD": true, // Send notification to hud (true - send, false - do not send), default - send
  "AdminNotifyDurationHUD": 5.0 // Duration of notification display in hud (only when AdminNotifyHUD = true), default 5.0 seconds
}
```

Installing the module:
+ Download the archive from the [releases](https://github.com/Stimayk/WestReportAdminNotify/releases)
+ Unzip to plugins
+ Customize the module in /configs/plugins
+ Customize translations if necessary
