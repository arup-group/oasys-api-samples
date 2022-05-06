# GsaConsoleApplication
This is an example console application that uses the GSA .NET API to output CSV files containing data from the model. It can be used as a starting point to develop custom applications for specific needs.

## Usage
The example application (once compiled) takes two command line arguments:
- `--input-file` (or simply `-i`) to specify the GSA file to open
- `output-directory` (or `o`) to specify the directoy to save the output CSV

An example run might look something like this:
```
GsaConsoleApplication.exe --input-file "C:\My model.gwb" --output-directory "C:\My CSV files"
```
This example app should produce a CSV containing some details of the model:
- Node data
- Element data
- Mamber data
- Custom 1D element results table

> Note, this console application uses the GSA .NET API, so GSA must be installed and the installation directory should be on the system's Path variable.

## Adapting
The application can be adapted to suit your needs. For example:
- command line options can be added or removed (see `CommandLineOptions.cs`)
- more simple data types (eg Node, Element and Member above) could be exported (see `ModelExporter.cs` for existing usage)
- custom exporters can be added or modified (see `BeamScheduleExporter.cs` for an example)
