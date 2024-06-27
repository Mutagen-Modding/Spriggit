using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Headers;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Meta;
using Noggog;
using Spriggit.CLI.Lib.Commands;

namespace Spriggit.CLI.Lib;

public static class StandardizeRunner
{
    public static async Task<int> Run(StandardizeCommand cmd)
    {
        using (var f = File.OpenWrite(cmd.OutputPath))
        {
            ModRecordSorter2.Sort(() =>
                {
                    var meta = ParsingMeta.Factory(BinaryReadParameters.Default, cmd.GameRelease, cmd.InputPath);
                    return new MutagenBinaryReadStream(cmd.InputPath, meta);
                }, f,
                cmd.GameRelease);
        }

        return 0;
    }
}

static class ModRecordSorter2
{
    public static void Sort(
        Func<IMutagenReadStream> streamCreator,
        Stream outputStream,
        GameRelease release)
    {
        using var inputStream = streamCreator();
        using var locatorStream = streamCreator();
        using var writer = new MutagenWriter(outputStream, release, dispose: false);
        if (inputStream.Complete) return;
        
        writer.Write(inputStream.ReadModHeaderFrame().HeaderAndContentData);
        while (!inputStream.Complete)
        {
            var grupLoc = inputStream.Position;

            var groupMeta = inputStream.GetGroupHeader();

            var storage = new Dictionary<FormID, List<ReadOnlyMemorySlice<byte>>>();
            using (var grupFrame = new MutagenFrame(inputStream).SpawnWithLength(groupMeta.TotalLength))
            {
                inputStream.WriteTo(writer.BaseStream, inputStream.MetaData.Constants.GroupConstants.HeaderLength);
                locatorStream.Position = grupLoc;
                foreach (var rec in ParseTopLevelGRUP(locatorStream))
                {
                    MajorRecordHeader majorMeta = inputStream.GetMajorRecordHeader();
                    storage.GetOrAdd(rec.FormID)
                        .Add(inputStream.ReadMemory(checked((int)majorMeta.TotalLength), readSafe: true));
                    if (grupFrame.Complete) continue;
                    if (inputStream.TryGetGroupHeader(out var subGroupMeta))
                    {
                        storage.GetOrAdd(rec.FormID)
                            .Add(inputStream.ReadMemory(checked((int)subGroupMeta.TotalLength), readSafe: true));
                    }
                }
            }

            foreach (var item in storage
                         .OrderBy((i) => i.Key.Raw))
            {
                foreach (var bytes in item.Value)
                {
                    writer.Write(bytes);
                }
            }
        }

        inputStream.WriteTo(writer.BaseStream, (int)inputStream.Remaining);
    }
    
    private static IEnumerable<(FormID FormID, long Position)> ParseTopLevelGRUP(
        IMutagenReadStream reader,
        bool checkOverallGrupType = true)
    {
        var groupMeta = reader.GetGroupHeader();
        var targetRec = groupMeta.ContainedRecordType;
        if (!groupMeta.IsGroup)
        {
            throw new ArgumentException();
        }

        reader.Position += groupMeta.HeaderLength;

        using (var frame = MutagenFrame.ByFinalPosition(reader, reader.Position + groupMeta.ContentLength))
        {
            while (!frame.Complete)
            {
                var recordLocation = reader.Position;
                MajorRecordHeader majorMeta = reader.GetMajorRecordHeader();
                if (majorMeta.RecordType != targetRec)
                {
                    var subGroupMeta = reader.GetGroupHeader();
                    if (subGroupMeta.CanHaveSubGroups)
                    {
                        reader.Position += subGroupMeta.TotalLength;
                        continue;
                    }
                    else if (checkOverallGrupType)
                    {
                        throw new ArgumentException($"Target Record {targetRec} at {frame.Position} did not match its containing GRUP: {subGroupMeta.ContainedRecordType}");
                    }
                }

                var len = majorMeta.TotalLength;
                yield return (
                    majorMeta.FormID,
                    recordLocation);
                reader.Position += len;
            }
        }
    }
}