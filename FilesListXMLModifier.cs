using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepIdentifier
{
    internal class FilesListXMLModifier
    {
        //Add
        //AllFilesUnder... should be updated for every build first..

        public void ResolveAddedFilesDependencies(List<string> addedFiles) 
        { 
            //Add it in the xml
            //Identify the app
            
        }

        //Modify
        public void ResolveModifiedFilesDependencies(List<string> addedFiles)
        {
            throw new NotImplementedException();
        }

        //Deletion

        public void ResolveDeletedFilesDependencies(List<string> addedFiles)
        {
            throw new NotImplementedException();
        }
    }
}
