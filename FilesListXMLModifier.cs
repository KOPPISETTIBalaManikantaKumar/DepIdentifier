using System.Xml;

namespace DepIdentifier
{
    internal class FilesListXMLModifier
    {
        //Add
        //AllFilesUnder... should be updated for every build first..
        //Get the changesets for everybuild and get these 3 lists.

        public List<string> AddedFiles = new List<string>();
        public List<string> ModifiedFiles = new List<string>();
        public List<string> DeletedFiles = new List<string>();

        public Dictionary<string, List<string>> m_DependencyDictionary = new Dictionary<string, List<string>>();

        public void ResolveAddedOrModifiedFilesDependencies(List<string> addedFiles)
        {
            int counter = 0;
            using (DynamicProgressBar progressForm = new DynamicProgressBar())
            {
                progressForm.SetMinAndMax(0, addedFiles.Count);
                //progressForm.Show();
                
                List<string> vcxProjFilesSelected = new List<string>();

                XmlDocument xmlDocument = XMLHelperAPIs.GetFilesListXmlDocument();

                foreach (var file in addedFiles)
                {
                    counter++;
                    progressForm.UpdateProgress(counter);

                    if (string.Compare(".vcxproj", Path.GetExtension(file), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        vcxProjFilesSelected.Add(file);
                        //m_filesForWhichDependenciesNeedToBeIdentified.Remove(file);

                        List<string> dependenicesOfCurrentFile = new List<string>();
                        if (m_DependencyDictionary.Keys.Any(key => key.Equals(file, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }
                        else
                        {
                            dependenicesOfCurrentFile = FileDepIdentifier.GetDependencyDataOfGivenFile(file);
                            dependenicesOfCurrentFile = dependenicesOfCurrentFile.Select(item =>
                                                item.Contains("..") && Path.IsPathRooted(item) ?
                                                Path.GetFullPath(item) : item.ToLower())
                                                .Distinct()
                                                .ToList();
                            m_DependencyDictionary.Add(file, dependenicesOfCurrentFile);
                            FileDepIdentifier.GetFileDependenciesRecursively(dependenicesOfCurrentFile);
                        }
                    }
                }

                addedFiles.RemoveAll(x => vcxProjFilesSelected.Contains(x) == true);

                foreach (var file in addedFiles)
                {
                    //Skip the other files for which we donot identify dependencies
                    if (!DepIdentifierUtils.IsFileExtensionAllowed(file))
                    {
                        if (!m_DependencyDictionary.Keys.Any(key => key.Equals(file, StringComparison.OrdinalIgnoreCase)))
                            m_DependencyDictionary.Add(file, new List<string> { "No Dependencies" });
                        continue;
                    }

                    DepIdentifierUtils.WriteTextInLog($"-->{counter}/{addedFiles.Count}");
                    List<string> dependenicesOfCurrentFile = new List<string>();


                    if (m_DependencyDictionary.Keys.Any(key => key.Equals(file, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }
                    else
                    {
                        dependenicesOfCurrentFile = FileDepIdentifier.GetDependencyDataOfGivenFile(file);
                        dependenicesOfCurrentFile = dependenicesOfCurrentFile.Select(item =>
                                                item.Contains("..") && Path.IsPathRooted(item) ?
                                                Path.GetFullPath(item) : item.ToLower())
                                                .Distinct()
                                                .ToList();
                        m_DependencyDictionary.Add(file, dependenicesOfCurrentFile);
                        FileDepIdentifier.GetFileDependenciesRecursively(dependenicesOfCurrentFile);
                    }
                }
                //progressForm.Close();
            }
        }

        //Deletion
        public void ResolveDeletedFilesDependencies(List<string> removedFiles)
        {
            foreach (var file in removedFiles)
            {
                XmlDocument xmlDocument = XMLHelperAPIs.GetFilesListXmlDocument();
                string referencesOfCurrentFile = XMLHelperAPIs.GetAttributeOfFilePathFromXML(xmlDocument, "References", file);
                if (!string.IsNullOrEmpty(referencesOfCurrentFile))
                {
                    List<string> referencesList = referencesOfCurrentFile.Split(";").ToList();
                    foreach(var reference in referencesList)
                    {
                        xmlDocument = XMLHelperAPIs.GetFilesListXmlDocument();
                        XMLHelperAPIs.RemoveReferencesFromDependecniesInXML(xmlDocument, reference);
                    }
                }
            }
        }
    }
}
