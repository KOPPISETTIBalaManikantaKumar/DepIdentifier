using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

public static class AsyncFileLock
{
    private static readonly Dictionary<string, SemaphoreSlim> Locks = new Dictionary<string, SemaphoreSlim>();

    public static async Task LockAsync(string filePath)
    {
        SemaphoreSlim semaphore;
        lock (Locks)
        {
            if (!Locks.TryGetValue(filePath, out semaphore))
            {
                semaphore = new SemaphoreSlim(1, 1);
                Locks.Add(filePath, semaphore);
            }
        }
        await semaphore.WaitAsync();
    }

    public static void Unlock(string filePath)
    {
        lock (Locks)
        {
            if (Locks.TryGetValue(filePath, out var semaphore))
            {
                semaphore.Release();
            }
        }
    }
}

//public static class DepIdentifierUtils
//{
//    public static async Task WriteListToXmlAsync(List<string> stringsList, string filePath, string rootElementName, string stringElementName, bool update = true)
//    {
//        try
//        {
//            // Acquire the file lock
//            await AsyncFileLock.LockAsync(filePath);

//            // Create a new XML writer and set its settings
//            XmlWriterSettings settings = new XmlWriterSettings
//            {
//                Indent = true, // To format the XML with indentation
//                IndentChars = "    ", // Specify the indentation characters (four spaces in this case)
//                Async = true // Enable asynchronous writing
//            };

//            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
//            {
//                // Write the XML declaration asynchronously
//                await writer.WriteStartDocumentAsync();

//                // Write the root element with the specified name asynchronously
//                await writer.WriteStartElementAsync(null, "FiltersData", null);
//                await writer.WriteStartElementAsync(null, rootElementName, null);

//                // Write each string as a separate element with the specified name asynchronously
//                foreach (string str in stringsList)
//                {
//                    await writer.WriteStartElementAsync(null, stringElementName, null);
//                    await writer.WriteAttributeStringAsync(null, "Name", null, str);
//                    await writer.WriteEndElementAsync();
//                }

//                // Close the root element asynchronously
//                await writer.WriteEndElementAsync();

//                // End the XML document asynchronously
//                await writer.WriteEndDocumentAsync();
//            }

//            DepIdentifierUtils.WriteTextInLog("XML file created successfully.");
//        }
//        catch (Exception ex)
//        {
//            DepIdentifierUtils.WriteTextInLog("Error writing XML file: " + ex.Message);
//        }
//        finally
//        {
//            // Release the file lock
//            AsyncFileLock.Unlock(filePath);
//        }
//    }
//}
