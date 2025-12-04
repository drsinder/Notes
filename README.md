# Notes
Current (as of 2026) iteration of Notes for the web. Based on PLATO Notes.

Notes Blazor/gRPC

Notes is a set of forums inspired by the PLATO (later known as NovaNET) communications system. 
The purpose of PLATO was to provide Computer Based Education facilities. The origin of PLATO goes back to 1960 
at the Computer-based Education Research Lab (CERL) at the University of Illinois. 
By 1973 a need was recognized for a means for users of PLATO to communicate in a durable and 
structured format. At this time David R. Woolley created "lesson" "notes". Mr. Woolley describes "notes" 
here: http://just.thinkofit.com/plato-the-emergence-of-online-community/ His original implementation remains 
available on Cyber1: https://cyber1.org/index.asp

"Notes" was rewritten in 1991 at the Computer-based Education Research Lab and on NovaNET. 
By this time Personal Computers (PCs and MACs) running a program known as "Portal" had replaced 
many of the original dedicated PLATO terminals. The functionality of the "Portal" exceeded that 
of the original PLATO terminals. "Notes" was showing it's age in a number of ways, some of which 
were related to lack of use of Portal functionality and others related to annoying 
limits of the original notes. The new "Notes" (notes 2) took advantage of many of the added Portal functionalities.

Notes 2 continued to function through August 31, 2015 at which time the then 
owner of NovaNET (Pearson Learning) shut down NovaNET.

This effort (Notes) is intended to continue the Notes tradition on a new platform. 
Many of the ideas and features of the original PLATO Notes are retained. Many of the Notes 2 advances are also retained. 
Finally Notes  is updated and rewritten again from the ground up to make 
appropriate use of the Web platform rather than a dedicated PLATO terminal or a NovaNET specific protocal Portal program.

The author of Notes managed and contributed to the Notes 2 rewrite on NovaNET.

A prior effort, 3.0, was written using Microsoft ASP.NET MVC 5. 
Version 2017 was derived from 3.0 but uses a new foundation, ASP.NET CORE 3, which permits greater flexibility. 
This version is built on dotNET Core 6. Notes also adds the ability to archive notes files (forums).  
Notes also goes beyond the capabilities of NovaNET notes to utilize the scrolling browser environment to advantage. 
For example, responses to notes have references related to what the author of the response was viewing when they made the response.  
These references can be displayed while viewing a note (response) and when composing a new response.
This version of Notes uses gRPC for client/server communications.

Client/Server (Hosted) app. WASM (HTML/C#/Blazor).

This is a Visual Studio 2026 project.

### Things you will need to develop this project:

- Visual Studio 2026 with dotNET 10 installed.
- SQL Server - Express, Developer Edition, or better.
- A SendGrid Account and an API Key.
- A Syncfusion community license key for Blazor. (https://www.syncfusion.com/)

### Things you will need to do to get started:
The Notes project "secrets.json" file with appropriate values:

Template secrets file is included in this repository...

Example you will need to fill in the values for your case:

{

  "ConnectionStrings:DefaultConnection": "Server=localhost;Database=Notes2026;Trusted_Connection=True;MultipleActiveResultSets=true",
  
 "JWTAuth": {
    "ValidAudienceURL": "https://localhost:7130",
    "ValidIssuerURL": "https://localhost:7130",
    "SecretKey": "Use a very long string here for your key"
  },
  "AppUrl": "https://localhost:7130",

  "ImportRoot": "E:\\Projects\\2022\\Notes2022\\Notes2022\\Server\\wwwroot\\Import\\",
  
  "PrimeAdminEmail": "youremail@wherever.com",
  
  "PrimeAdminName": "Your_Name",
  
  "SendGridApiKey": "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  
  "SendGridEmail": "youremail@wherever.com",
  
  "SendGridName": "Your Name on behalf of Notes",

  "SyncfusionKey": "xxx"

  
}

- PrimeAdminEmail will become an Admin when the email address is used for registeration.
- SendGridApiKey: Your sendgrid api key
- SendGridEmail: Email from the app will be from this address.
- ConnectionStrings:DefaultConnection: Modify if needed for your situation. 

Use Visual Studio to perform an Update-Database in the Package Manager Console.

Start debugging the app.  Register, confirm your email when the message arrives (keep app running for this), Log in.
From the Admin menu item choose NoteFiles.  Then add a few note files to work with.  Some buttons are available to add some standard ones.

Write some notes!


### Solution organization
The Solution has 2 projects
- Notes.Client : All of the client side function resides here as components and @page s..
- Notes        : Direct access to Db and provides data and operations to Client through the gRPC.
