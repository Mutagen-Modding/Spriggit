using CommandLine;
using Spriggit.CLI;
using Spriggit.CLI.Commands;

var runner = new Runner();

return await Parser.Default.ParseArguments(args, typeof(DeserializeCommand), typeof(SerializeCommand))
    .MapResult(
        async (DeserializeCommand deserialize) => await runner.Run(deserialize),
        async (SerializeCommand serialize) => await runner.Run(serialize),
        async _ => -1);