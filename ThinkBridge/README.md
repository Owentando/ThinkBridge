\# ThinkBridge



An AI-powered e-learning platform built with ASP.NET MVC 5.



\## Tech Stack

\- ASP.NET MVC 5 / .NET Framework 4.8

\- Entity Framework 6 (Code First)

\- SQL Server LocalDB

\- Bootstrap 5, jQuery 3.7

\- BCrypt password hashing

\- Markdig Markdown rendering



\## Getting Started



\### Prerequisites

\- Visual Studio 2022

\- SQL Server LocalDB (comes with Visual Studio)

\- .NET Framework 4.8



\### Setup Steps

1\. Clone the repo

2\. Open `ThinkBridge.slnx` in Visual Studio 2022

3\. Right-click the solution → Restore NuGet Packages

4\. Copy `ThinkBridge/Web.config.example` to `ThinkBridge/Web.config`

5\. Fill in your API keys in `Web.config`

6\. Open Package Manager Console → run `Update-Database`

7\. Press F5 to run



\## API Keys Required

Add these to your local `Web.config` (never commit this file):

\- `OpenTurnerApiKey` - OpenRouter AI API key

\- `OpenTurnerApiUrl` - OpenRouter API URL

\- `OpenTurnerModel` - AI model name

\- `GoogleSearchApiKey` - Google Custom Search API key

\- `GoogleSearchCx` - Google Custom Search Engine ID



\## User Roles

\- \*\*Admin\*\* - manages users, subjects, system activity

\- \*\*Lecturer\*\* - creates content, quizzes, video lessons

\- \*\*Student\*\* - learns, takes quizzes, uses AI tutor



\## Features

\- AI Tutor chat

\- AI-generated summaries

\- Quiz engine with scoring

\- Study planner and progress tracking

\- Personal notes

\- Study rooms

\- Offline saved content

\- Course materials and video lessons

