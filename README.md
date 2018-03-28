# JustABackup
A backup solution written in C# and ASP.NET Core, that bases its connections to sources and targets through plugins.
Currently its hosted by the Kestrel host in ASP.NET Core with a UI as a webpage.
Currently in a very early stage in development. Feedback and ideas is much appreciated.

### Table of Content
* The idea
* TODO's
* Installation
* Creating a plugin

## The idea
Currently the backup solution is based mainly on three provider types. **Backup**, **Transform** and **Storage**.
A **backup** provider provides a list of files that should be backed up, for example a folder on a disk or a database.
A **transform** provider will modify the input file or files before being backed up. A transform might be multiple files being zipped into a single file.
A **storage** provider will store files at a target connection, for example a folder or an online storage area like OneDrive.

These three providers will together form a backup **job**, that will run on a schedule.
A **job** is configured with one backup provider, zero or many tranform providers and a one storage provider.
When a job is triggered by its schedule it will read all files from its backup provider, transform them with the configured transform provider (if any) and at last send them to the storage providers.

By design the providers do not know about each other and therefor the user

#### Example
Every day at 03:00 AM the folder /important/data will be read, zipped into a single file (backup.zip) and that zip file will be encrypted (backup.zip.enc), and lastly it will be stored in OneDrive.

---

## TODO's
### Planned features
* Rewritten UI with a more API based framework.
* Plugin-store, to download plugins from a repository online.
* Testable providers, for testing connection to its source.
* View logs in the UI.
* Validation of provider properties, like required, range and regex.
* Host as a service in Windows.
* Notification system in the UI, to get notification if jobs started, completed and/or generated error.
* Notification providers, to be able to be notified when a job is completed through for example SMTP or Slack.
* and more..

### Known issues
* As the application is in an early stage, there should be some unknown bugs :)
* Some plugins have dependencies on .Net Framework DLL's, and might not work on other platforms than Windows at the moment.

---

## Installation
1. Install the dotnet core runtime
    * Can be downloaded here https://www.microsoft.com/net/download
2. Unpack the .zip found under releases.
3. run the command "dotnet JustABackup.dll" (or run.bat) to start the application.
4. Open your browser and go to http://localhost:5000/.

---

## Creating a plugin
The first step to creating a plugin is adding a reference to JustABackup.Base.dll.
Depending on which provider you want to implement you need to implement the following interfaces.
When you are done, just compile and add the DLL's to the root directory. Then just restart the application.

### Backup provider
```csharp
namespace JustABackup.Base
{
    /// <summary>
    /// A provider that will supply files to be backuped.
    /// Files read by this provider will later be stored by a storage provider.
    /// Note: The purpose for this provider is to be read-only.
    /// </summary>
    public interface IBackupProvider : IDisposable
    {
        /// <summary>
        /// Gets a list of files to backup.
        /// Example: return data from File.GetFiles(folder);
        /// </summary>
        /// <param name="lastRun">Timestamp when the last successfull backup started.</param>
        /// <returns>A list of files to backup.</returns>
        Task<IEnumerable<BackupItem>> GetItems(DateTime? lastRun);

        /// <summary>
        /// Opens a readable stream to a file, returned by the GetItems method.
        /// Note: The stream should not be closed or disposed before beging returned.
        /// Example: return the stream from File.OpenRead(path);
        /// </summary>
        /// <param name="item">The file to open a stream to, returned by the GetItems method.</param>
        /// <returns>An open stream to a file.</returns>
        Task<Stream> OpenRead(BackupItem item);
    }
}
```

### Storage provider
```csharp
namespace JustABackup.Base
{
    /// <summary>
    /// A provider that will write files to a storage.
    /// Files stored by this provider can come directly from a backup provider or have been modified by a transform provider.
    /// Note: The purpose for this provider is to be write-only.
    /// </summary>
    public interface IStorageProvider : IDisposable
    {
        /// <summary>
        /// Writes a file to the storage, from the open stream.
        /// Note: The stream can be modified by a transform provider before being delivered from a backup provider.
        /// </summary>
        /// <param name="item">The metadata of the file to be written.</param>
        /// <param name="source">An open stream to the file to write.</param>
        /// <returns>If the file was written successfully.</returns>
        Task<bool> StoreItem(BackupItem item, Stream source);
    }
}
```

### Transform provider
```csharp
namespace JustABackup.Base
{
    /// <summary>
    /// A provider that will transform files before being written to a storage provider.
    /// Example: Write multiple files from a backup provider to a zip file before being sent to a storage provider.
    /// </summary>
    public interface ITransformProvider : IDisposable
    {
        /// <summary>
        /// Map files from previous step to how the output file should look like after this transform provider.
        /// Example 1: Multiple files turn to a single .zip file.
        /// Example 2: Single file to multiple output files, one original file and one crc file.
        /// </summary>
        /// <param name="input">List if files from the previous step.</param>
        /// <returns>A mapped list of files.</returns>
        Task<MappedBackupItemList> MapInput(IEnumerable<BackupItem> input);

        /// <summary>
        /// Transforms a single or list of files into a single output.
        /// This method will be called based on the output from the MapInput method.
        /// Output file should be written to the outputStream parameter.
        /// Example 1: Multiple files will be added to a .zip file.
        /// Example 2: Single file will be encrypted,
        /// </summary>
        /// <param name="output">The file that will be outputed.</param>
        /// <param name="outputStream">Stream that the output file will be written to.</param>
        /// <param name="inputFiles">File(s) to transform.</param>
        /// <returns>An awaitable task.</returns>
        Task TransformItem(BackupItem output, Stream outputStream, Dictionary<BackupItem, Stream> inputFiles);
    }
}
```

### Adding properties
Providers support custom properties, which are defined as properties in the Provider class.
The properties also support attributes to define how the property should behave or be shown in the UI.

```csharp
namespace DemoPlugin
{
    public interface DemoProvider : IBackupProvider
    {
        [Display(Name = "Temporary Folder")]
        public string TemporaryFolder { get; set; }
    }
}
```

Currently supported property types:
* string
* int
* bool

The following attributes are currently supported:
* DisplayNameAttribute (Provider classes)
* DisplayAttribute (Properties)
* PasswordPropertyTextAttribute (Properties)
* TransformAttribute (Properties) [Custom]

---