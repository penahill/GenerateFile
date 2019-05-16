# GenerateFile
This program uses store procedures in Sql Server and writes it to a csv file.

This is a working production program and the app.config file has purposely not been included. This program will not run without this information. 

This program takes up to 2 parameters, warehouse# and program name where the program name is optitional. 
If the program name is not passed in the default will to run all of the programs.

Each program will pass parameters into the store procedures returning the data into the appropriate class. This data is then looped through and uses the .NET CsvHelper utility to write the csv file. 

