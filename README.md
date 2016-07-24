# Windows-Server-Monitor
Allow memory and disk space monitoring on Windows based server. Can be used in cloud base servers where no alerts are available.

Edit the App.config file and add your info:\r
SLACK_URL - a slack webhook url\r
MACHINE - the name of the machine - use this to track the server\r
TO_EMAILS - a comma seperated list of email address to send the notification.\r
AVAILABLE_FREE_SPACE_THRESHOLD_IN_MB - Get an alert when a drive is below X\r
AVAILABLE_FREE_MEMORY_THRESHOLD_IN_PERCENTAGE - Get an alert when the memory is below X% (Available memory divided by Total\r memory)
\r
The best way to use this program is to schedule a task in the Task Scheduler that run every 5 minutes (or less) and run the\r program. Make sure to make it run daily every X minutes so it will not stop after 1 day.\r

