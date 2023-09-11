
The ReversePatcher tool gives the list of compile time dependencies of the selected files.

The default option is to select from the filters and list the dependencies

*It can also be selected from Tools->Select files from filters.

1. Select the App from the filter combo
2. Select the required files from the list.
3. Click on Show selected files
4. Click on Get Dependencies
5. Once the identification or fetching the list is done, the Complete dependency list or the dependency tree will be displayed.
6. Copy to clipboard copies all the identified dependencies into the clipboard.

*Tools->Input files in Text

This accepts the input files for which it needs to identify the dependencies in the text format.
1. Add the files which are in the groot and seperated by line space.
Ex:
g:\mroot\CommonApp\Testfolder\file1.cpp
g:\mroot\CommonApp\Testfolder2\file2.cpp

3. Click on Show selected files
4. Click on Get Dependencies
5. Once the identification or fetching the list is done, the Complete dependency list or the dependency tree will be displayed.
6. Copy to clipboard copies all the identified dependencies into the clipboard.

*Tools->Add new files

This accepts the input files which are newly added in the product for which we need to identify the dependencies.

1. Add the files which are in the groot and seperated by line space.
Ex:
g:\mroot\CommonApp\Testfolder\file1.cpp
g:\mroot\CommonApp\Testfolder2\file2.cpp

3. Click on Show selected files
4. Click on Get Dependencies
5. Once the identification or fetching the list is done, the Complete dependency list or the dependency tree will be displayed.
6. Copy to clipboard copies all the identified dependencies into the clipboard.


*Tools->Remoce files

This is useful for removing the files which are removed from the product and to update the data.
1. Add the files which are in the groot and seperated by line space.
Ex:
g:\mroot\CommonApp\Testfolder\file1.cpp
g:\mroot\CommonApp\Testfolder2\file2.cpp

3. Click on Show selected files
4. Click on Remove Files

*Tools->Genereate Pre-requisite Files

This is used to create the prerequisite data required for this tool using the S3DPatcher file which is mentioned in the app.config file.

*Tools->References

This is used to see the file references in all the other files in the product

1. Select the App from the filter combo
2. Select the required files from the list.
3. Click on Show selected files
4. Click on Get references
5. Once the identification or fetching the list is done, the Complete reference list or the dependency tree will be displayed.
6. Copy to clipboard copies all the identified references into the clipboard.


