using System.Globalization;
using System.IO.Abstractions;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.IO.DI;
using Noggog;
using Noggog.IO;

namespace Spriggit.Engine;

public class PluginBackupCreator
{
    private readonly IProvideCurrentTime _currentTime;
    private readonly IFileSystem _fileSystem;
    private readonly IModFilesMover _modFilesMover;
    private const string DateFormat = "MM-dd-yy hh-mm-ss-fff";

    public PluginBackupCreator(
        IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        IModFilesMover modFilesMover)
    {
        _currentTime = currentTime;
        _fileSystem = fileSystem;
        _modFilesMover = modFilesMover;
    }
    
    private void Clean(DirectoryPath path, uint backupDays)
    {
        if (!path.CheckExists(_fileSystem)) return;

        var now = _currentTime.Now;
        var maxDiff = TimeSpan.FromDays(backupDays);
        
        foreach (var backupDir in path.EnumerateDirectories(includeSelf: false, recursive: false, _fileSystem))
        {
            if (!DateTime.TryParseExact(backupDir.Name, DateFormat, null, DateTimeStyles.None, out var dt)) continue;
            var diff = now - dt.Date;
            if (diff > maxDiff)
            {
                backupDir.DeleteEntireFolder(deleteFolderItself: true, fileSystem: _fileSystem);
            }
        }
    }
    
    public DirectoryPath? Backup(ModPath path, uint backupDays)
    {
        if (backupDays == 0) return null;

        using var tempDirForMod = TempFolder.FactoryByAddedPath(
            Path.Combine("Spriggit", "Backups", path.Path.Name), 
            deleteAfter: false,
            deleteBefore: false, 
            fileSystem: _fileSystem);

        Clean(tempDirForMod.Dir, backupDays);

        if (ShouldShortCircuit(path, tempDirForMod.Dir, out var shortCircuitPath)) return shortCircuitPath?.Directory;
        
        return MakeBackup(path, tempDirForMod);
    }

    private DirectoryPath MakeBackup(ModPath path, TempFolder temp)
    {
        var dt = _currentTime.Now;
        var dir = new DirectoryPath(Path.Combine(temp.Dir, dt.ToString(DateFormat)));
        dir.Create(_fileSystem);
        _modFilesMover.CopyModTo(path, dir, overwrite: false);
        return dir;
    }

    private bool ShouldShortCircuit(
        FilePath sourcePath,
        DirectoryPath backupDir,
        out FilePath? shortCircuitPath)
    {
        if (!_fileSystem.File.Exists(sourcePath))
        {
            shortCircuitPath = default;
            return true;
        }
        
        foreach (var specificBackup in backupDir
                     .EnumerateDirectories(includeSelf: false, recursive: false, _fileSystem)
                     .OrderBy(d =>
                     {
                         if (DateTime.TryParseExact(d.Name, DateFormat, null, DateTimeStyles.None, out var dt))
                         {
                             return dt;
                         }

                         return new DateTime();
                     }))
        {
            var path = Path.Combine(specificBackup, sourcePath.Name);
            if (!_fileSystem.File.Exists(path))
            {
                shortCircuitPath = default;
                return false;
            }
            if (FilesAreEqual(path, sourcePath))
            {
                shortCircuitPath = path;
                return true;
            }
            else
            {
                shortCircuitPath = default;
                return false;
            }
        }

        shortCircuitPath = default;
        return false;
    }
    
    // https://stackoverflow.com/questions/1358510/how-to-compare-2-files-fast-using-net
    const int BYTES_TO_READ = sizeof(Int64);
    
    public bool FilesAreEqual(FilePath first, FilePath second)
    {
        var firstInfo = _fileSystem.FileInfo.New(first.Path);
        var secondInfo = _fileSystem.FileInfo.New(second.Path);
        if (firstInfo.Length != secondInfo.Length)
            return false;

        if (string.Equals(first.Path, second.Path, StringComparison.OrdinalIgnoreCase))
            return true;

        int iterations = (int)Math.Ceiling((double)firstInfo.Length / BYTES_TO_READ);

        using (var fs1 = first.OpenRead(_fileSystem))
        {
            using (var fs2 = second.OpenRead(_fileSystem))
            {
                return fs1.ContentsEqual(fs2);
            }
        }
    }
}