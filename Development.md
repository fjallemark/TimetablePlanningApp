# Developent plan
This is the development plan for the *Timetable Planning App*.

*By Stefan Fjällemark*
## Background
I started with planning schedules for module meetings in 2017. 
I was presented to XPLN - the FREMO planning tool - based on *OpenOffice*. 
In my opinion, building user applications in a spreadshet software is mostly an anomaly,
especially if it contains complex data structures. 
However, it is the defacto standard with many skilled users.

Being a proffessional developer with lot of experiece designing and building public transport software and databases,
I never considered using XPLN, but started directly by building schedules in Microsoft Access, 
a quite competent desktop database application.

Since 2017, for each plan I did, the database design was refined and extended with new or improved reports. 
The status of the database in March 2020 was that further development required a new approach. 
Along with the development in Microsoft Access, the idea to create a cloud based scheduling system growed.
But how to do this in steps that gives short time benefits,
while at the same time supports the longer perspective of a cloud based solution? 
My answer was to implement it in small steps with direct benefits to the Access databas but where the featurens
and the technology behind it could be transferrerd to a cloud solution with no or minimal change.

## Development strategy
There are two factors to consider:
1. XPLN is the defacto standard for most schedule planning today. 
So it is important that work done in XPLN also can bebefit from the short term investments. 
2. My database approach has been proved to work.
The data model has been extended and verified in a number of advanced planning scenarios. 
This data model forms the future cloud database.

### Short term goals
It is important that all investments should pay off in the short time perspective.
This means that new things that are developed should work with the Microsoft Access database until a cloud database is a reality.
It is also very easy to prototype tables and views in Microsoft Access. 
This results a database design that can be implemented in a cloud database.

Important tasks in the short term:
1. Remove special dependencies of the Access database. 
In particular, all reports have to be replaced by a solution that works with Access and a future cloud database without modifications.
2. Integration of XPLN, by supporting import of scheduling data from XPLN-spreadsheets to the Access database and later the could database.
This has already been prototyped with good results.

### Medium term goals
The next step is to develop a user interface for entry and modification of schedules. 
Propably this will first be developed against a local SQL database and no longer use the Access database
It is important that the local database later easy can be promoted to a cloud database.

### Long term goals
Cloud based scheduling system with user authentication, 
user interface for scheduling and reports, 
a WEB API to retrieve data to other purposes.

## Technical strategy

### Application structure
The user application will run locally in the users browser. This improves the user experience.
The user application communicates with the backend using a WEB API. 
The WEB API will also be read only for other types of clients, 
so all scheduling data will be available as JSON or XML.

The server application handles authentication and authorisation, database reads and writes, 
WEB API requests and responses and localization of API-data.

Both user- and server applications are from start designed to support several languages.

### Terminology
The terminology in the system is primarily based on the european reference datamodel [TRANSMODEL](http://www.transmodel-cen.eu/).
For certain railway terminology other eurupean sources are used that supplements TRANSMODEL.
Because it is a highly specialised terminology, it is a challenge to translate these terms to other langauages.

### Platform
Both user- and server applications are developed on the .NET platform, which is choosen for a number of reasons:
1. I have a long and good experience with this platform.
1. It runs on many operating systems: Windows, MacOS, Linux (several distributions) and on mobile platforms.
1. The [ASP.NET web framework](https://dotnet.microsoft.com/apps/aspnet) is very performant.
1. The [Blazor framework](https://blazor.net) enables writing .NET also in the browser that runs i [WebAssembly](https://webassembly.org/).
1. It is all open source: the compilers, the base class library, ASP.NET.
1. There are free but very competent development tools like [Visual Studio](https://visualstudio.microsoft.com/).
1. Using C# makes it not so hard for Java developers to understand code.

Note that .NET also is used for the client, that runs in the browser. The client is written in C#, HTML and CSS,
and NO JavaScript. Therefore, code can be shared between the server and client, something that makes
a developer life a lot more easy thanks to [Blazor](https://blazor.net).

Currently, development is on [.NET Core 3.1](https://dotnet.microsoft.com/). An upgrade to [.NET 5](https://devblogs.microsoft.com/dotnet/introducing-net-5/) is expected during spring 2021.

## Status August 2020
The work to replace Access reports have made a significat progress. 
The prototyping of using HTML/CSS for prettyprinting and pagination has been sucessful. 
Delivering data to the user app over WEB API for printing has worked as a charm.
Printing as also given less sheets to print for the 52x92mm items, because Access was limited to 5 items per page.

The following reports have been implemented as HTML/CSS reports indended for printing on paper: 
1. **Loco schedules**: prints ten 52x92mm schedules per page with red diagonal line.
2. **Trainset schedules**: prints ten 52x92mm schedules per page with green diagonal line.
3. **Waybills**: prints ten 52x92mm waybills per page. Coloring based on origin and destination regions as defined in the database.
4. **Driver duties**: prints A5 pages ordered for booklet printout. 
This report is defenitley an improvement in both speed and layout compared to the Access report.
The page ordering simplifies booklet printing on printers with double side printing.
Even if many driver instructions were created from data in Access, 
the new driver duty report has improved the intructions and they are also generated in the preferred language.
5. **Translations** from english to swedish and danish is made for the implemented reports.

You find some examples in PDF in the [Examples folder](https://github.com/tellurianinteractive/Tellurian.Trains.TimetablePlanningApp/tree/master/Examples).



