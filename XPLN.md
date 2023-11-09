## Handling XPLN data
XPLN is the defacto tool to create schedules and printed media for module meetings.
It is developed based on *OpenOffice Calc*, with scripting and forms. 
Because it lacks the data integrity of a real database, it requires users to
follow a strict workflow to not end up with inconsistent data.

#### Reading XPLN data 
Because data is stored in worksheet in a understandable format,
it is possible to let software read out the XPLN data.
I have built such software that reads out all possible data; 
stations, trains, loco turnus and duties etc.

When reading the XPLN-data an building into a model in memory, 
I made the following experineces:
- Almost every XPLN-document suffers from inconsistent data,
which makes further reading impossible until the XPLN-document has been correctred.
- Valid but unreasonable data, for example two trains scheduled at the same time on the same single-track stretch, 
or to fast/slow running times between stations.

#### Reading XPLN-documents
XPLN documents have the file type .ODS, which stands for *Open Document Spreadsheet*.
The content of an .ODS-file is a rather complex XML. 
Reading the raw XML is therefore out of the question and no suitable free NuGet.package exist for reading .ODS-files.

However, reading .ODS-files is supported if you have Microsoft Excel installed.
Using COM-interop it is fairly easy to open and manipulate Excel-files programmatically from a .NET application.
It is possible to open an .ODS-file with Excel, and then read its content. And thats the current approach.

There also exists solutions to read Excel-files <u>not</u> having Excel installed, using a free NuGet package 
*DocumentFormat.OpenXml*. But this is more tricky, so this approach will be considered to use in a cloud solution,
where it is not possible to depend on COM-interop with Microsoft Excel.

I finally found an old piece of C# code to read .ODS-files without Excel and COM-imterop.
This was tested on a large number of XPLN-files an refined to read all data correct.
This solution can also run in the cloud. Try it out by [uploading you XPLN-file](https://timetableplanning.azurewebsites.net/xpln) and get a validation report.
