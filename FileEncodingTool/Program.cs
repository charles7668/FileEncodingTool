// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Text;
using EncodingChecker;
using UtfUnknown;

var rootCommand = new RootCommand("Check file encoding")
{
    new Option<string>(
        new[] { "--directory", "-d" },
        Directory.GetCurrentDirectory,
        "target directory"),
    new Option<string>(
        new[] { "--target", "-t" },
        () => "",
        "Convert target encoding; if not specified, only detect files."),
    new Option<string[]>(
        new[] { "--patterns", "-p" },
        "To detect files with a search patterns."
    ) { IsRequired = true, AllowMultipleArgumentsPerToken = true },
    new Option<bool>(new[] { "--sub-dir" },
        () => true,
        "Include subdirectories, default is true.")
};

rootCommand.Handler = CommandHandler.Create(
    async (string directory, string target, string[] patterns, bool subDir) =>
    {
        if (patterns.Length == 0)
        {
            await Console.Error.WriteLineAsync("pattern is not set");
            return 1;
        }

        Encoding? targetEncoding = null;
        if (!string.IsNullOrEmpty(target))
            try
            {
                targetEncoding = Encoding.GetEncoding(target);
            }
            catch (Exception)
            {
                await Console.Error.WriteLineAsync("target encoding is not support");
                return 1;
            }

        DirectoryInfo dirInfo = new(directory);
        foreach (var pattern in patterns)
        {
            var fileInfos = dirInfo.GetFiles(pattern,
                subDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (var fileInfo in fileInfos)
            {
                var detect = CharsetDetector.DetectFromFile(fileInfo.FullName);
                var detectDetail = detect.Detected;

                Encoding? srcEncoding;
                // if known encoding, use detected encoding
                if (detectDetail.EncodingName != "windows-1252" && detectDetail.Confidence >= 0.7)
                {
                    Console.WriteLine($"{fileInfo.FullName} Detect : {detectDetail.EncodingName}");
                    srcEncoding = detectDetail.Encoding;
                }
                else
                {
                    await Console.Error.WriteLineAsync($"{fileInfo.FullName} Detect fail");
                    continue;
                }

                if (targetEncoding == null) continue;
                var content = await File.ReadAllBytesAsync(fileInfo.FullName);


                content = Encoding.Convert(srcEncoding, targetEncoding, content);
                if (Equals(targetEncoding, Encoding.UTF8) || Equals(targetEncoding, Encoding.Unicode) ||
                    Equals(targetEncoding, Encoding.UTF32))
                    content = EncodingHelper.RemoveBOM(content);
                await File.WriteAllBytesAsync(fileInfo.FullName, content);
                Console.WriteLine($"Converted {fileInfo.FullName} to {targetEncoding.BodyName}");
            }
        }

        return 0;
    });

var errorCode = await rootCommand.InvokeAsync(args);
Console.WriteLine($"Complete with error code {errorCode}");
return errorCode;