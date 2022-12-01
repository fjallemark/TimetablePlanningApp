# Timetable Planning App
>Last update: 2022-04-16

This is the **experimental version** of the web application for 
working with scheduling of model railway operation at module meetings.

This experimental version has been developed step-by-step after gaining practical experiences
and user input at several module meetings in Sweden and Norway during the last five years.

### Background
It based on a **Microsoft Access** database, and development started in 2016. 
I was initial using the built in report functionality to create all outputs.
This has some limitations and performance issues.
It also was non-web, a complete local single-user, single-language solution. 

### Current Development
The overall goal is to create an online scheduling system as a cloud application.
The focus now is to move and extend reporting/printing functionality to use standard web concepts; 
HTML and CSS and to retrive all data through a WEB API.
In addition, everything new should have multi-language support from start.
This makes it possbible to refine all printing features first and still using the Access database,
and learning what data needed for schedule planning, 
before moving also the planning features and the database to the cloud.


In parallel, the [**Module Registry**](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp), is developed to support all necessary data
for planning module meetings and supply a future fully cloud based solution for
planning schedules. The Module Registry is in operation and development is mostly completed.

### Features
This experimental version has **currently** a set of features, some of them not found in any other
model train planning software.

##### Basic Stuff
All data is entered directly in Access tables, so no fancy user interface yet to enter data.
However, Access supports hierarcial data entry, so it is not as bad as it seems.
The good part with Access is the ease to prototype the data structures and the views to support 
content to all reports. 

##### Database Consistency and Integrity
Storing data in a *relational database* means you can enforce data consistency and integrity much better,
compared to store data in spreadsheets or plain text files. 
The most common planning system within module railway operations planning is XPLN. 
It stores data in spreadsheet, and [**here**](https://github.com/fjallemark/TimetablePlanningApp/blob/master/XPLN.md) 
you can read my experiences when trying to import XPLN-data into
the Access database.

##### Operation Days
For each train, loco, trainset and duty an operation day can be specified.
This enables flexible planning, for example trainset schedules over several days.
It can be used when game sessions are assigned a running weekday.

##### Graphical Timetable **NEW**
A sequence of stations forms a *timetable stretch*. 
Timetable stretches may overlap each other.
Each timetable stretch prints as a separate *grapic timetable*,
with all trains passing som part of the timetable stretch.
Also track occupancy of any train is shown.

When trains are operated on different days, the graphical timtable can be blurred with details.
It is therefore possible to print the graphical timetable valid for only a subset of operating days.

##### Graphical Loco Turnus
Shows the usage of all locomotivs.
Each loco displays on a row, with bars showing when the loco is used in a train.
In order to see the true occupancu of the loco, the lead-in and lead-out time is also showed.
Lead-in time is the trains arrival time at the first station, 
and lead-out is the trains departure time from the last station.

##### Automatic Notes to Loco Driver and Station Staff
One of the hassles with other planning systems, is entering notes to loco drivers and station staff.
In this solution, most aspects of the train operation is entered as data and not as free text, 
which means you seldom enter any manually written notes.
Instead, the system creates all notes on report generation. 
Examples of automatic notes are:
- If train not stops at a station.
- Train meets at stations checked to show this.
- Where to couple or uncouple locos, trainsets or freigth wagons to specific destinations.
- **NEW** What to do at arrival to an unmanned shadow station.
- If to perform local shunting .
- If locos should be reversed, turned or exchanged at some station.
- The operation day (i.e. weekday) of when the note apply. 
The weekday notes consider the union of the operation days of the train, loco/trainset, and the duty,
and if they not overlap, the note is excluded, and if it applies to all of the duty days, the operating days are excluded from the note.
- Automatic notes also are assigned any combination of target auduence: *loco driver*, *train dispatcher* and/or *shunting staff*.
For example, only notes intended for loco drivers appear in the loco driver duties.
- Automatic notes are also applied to an arrival or a departure.

The advantages with automatic notes are that:
- you don't have to write them yourself, just click some checkboxes or enter some data,
- they are formulated in a consistent way, 
- they have the same formulation in every report,
- they are applied contextually in a predefine and systematic way,
- they can easily be generated in any supported language.

##### Manual Notes
Even if it is rarley needed,
it is possible to enter manually written notes and specify whether they apply to
*loco driver*, *train dispatcher* and/or *shunting staff*
and if they apply to an *arrival* or *departure* at the station.
You can apply which operating days a manual note should be valid
Manual notes can also be written in additional languages.
Currently, English, German, Danish, Norwegian and Swedish are supported.

##### Multilanguage Support
All reports can be printed in any of the supported languages. 
This is currently controlled by your preferred language in your browser settings.
All notes are also printed in the preferred language.

The next step is to specify the preferred language of each duty. 
This could be handy at an international meeting where trains are operated in different countries,
and the duties are in the language of the country it is carried out.

The solution can also be extended to support a new language, 
simply by translating the English version of all texts.
All text are kept separate from the rest of the application in order to faciliate translations.

##### Loco- and Trainset Schedules
These schedules can be planned with great flexibility,
for example different schedule depending on operation day.
One or more schedules can then be assigned to a specifc loco or trainset.
Each loco and trainset schedule are printed per operation day,
so there might be two or more schedules than you have to change before the next session starts.

##### Train Start Labels
For each initial train it is possible to print out a start label,
that can be placed on the track ahead of the train to make it easier to find.

##### Driver Duties
A driver duty printed in A5 booklet format contains:
- a first page with some general information about the duty,most of it are deducted from the trains in the duty.
- an optional second page with general instructions for the operated layout.
- a page for each train to drive. If the duty has more than one train, it writes a red highlight to continue to next train.
- eventually empty pages with text *intended empty page*.

All pages are numbered, also the 'empty' ones. 
They are printed in booklet order, which means easy print on A4 paper double-sided and fold.
This works regardless of the number of pages.

##### Station Duties
A station duty printed in A5 booklet format contains:
- a first page with some general information about the duty, if its a *dispatcher duty*, a *shunting duty* or a combined.
The combined is intended for single manned stations.
- an optional second page with general instructions for the operated layout, same as in driver duties.
- an optional station specific free text instructions. These are entered for each station and for dispatcher and shunting staff separately.
- one to several pages with trains that require some special attention (don't just passes through).
- eventually empty pages with text *intended empty page*.

All pages are numbered, also the 'empty' ones. 
They are printed in booklet order, which means easy print on A4 paper double-sided and fold.
This works regardless of the number of pages.

##### Block Planning
It is possible to plan all cargo flow by specifying what destinations to pick up and let off at each station.
Each cargo flow destination is called a *block*. There are also features for:
- specifying to bring wagons *to all destinations* or to a station *and beyond*,
- if train does not pass the final destination of a cargo flow, it could have a *transfer distinations* as it final destination,
- if train driver shall do local shunting or not. If not, local shunting is made by station staff.
- ordering *blocks* within a train.

The **block planning report** shows the order of the wagon groups (blocks) should be in a train from a specific station
It also specifies the maximum number of wagons in each block.
A block destination can be a single station or a set of stations.

##### Regions Attached to Shadow Station
Each shadow station can have one or several regions attached to it. 
Regions defines what geographical area the shadow station represents of the 'outside world' in relation to the meeting layout.
The *block planning* also consider the regions as destinations when a block ends at a shadow station and has *and beyond* checked.

### Data Export
Some station owners want data instead (or in supplement) of paper.
The data export creates a CSV-file, with all trains passing the station, including all station notes.
The output can be created in any supported language.

### Report Examples
You will find examples of some reports (in English) [**here**](https://github.com/fjallemark/TimetablePlanningApp/tree/master/Examples).
