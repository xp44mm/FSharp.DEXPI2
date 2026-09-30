using System;

namespace Dexpi2CppGen;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Dexpi2CppGen <input.xmi> <outputDir> [reportPath]");
            return 2;
        }

        var input = Path.GetFullPath(args[0]);
        var outputDir = Path.GetFullPath(args[1]);
        var reportPath = args.Length >= 3 ? Path.GetFullPath(args[2]) : Path.Combine(outputDir, "generation-report.md");

        if (!File.Exists(input))
        {
            Console.Error.WriteLine("Input file not found: " + input);
            return 1;
        }

        var model = XmiParser.Parse(input);
        var report = new GenerationReport();
        var emitter = new CppEmitter(model, report);
        var files = emitter.Emit(outputDir, input);

        Console.WriteLine("Input:      " + input);
        Console.WriteLine("SHA-256:    " + emitter.SourceSha256);
        Console.WriteLine("Packages:   " + model.Packages.Count(p => p.Types.Count > 0) + " generated namespace(s)");
        Console.WriteLine("Classes:    " + report.ClassCount + " (abstract " + report.AbstractCount + ")");
        Console.WriteLine("Enums:      " + report.EnumCount);
        Console.WriteLine("Properties: " + report.PropertyCount);
        Console.WriteLine("Associations: " + report.AssociationCount);
        Console.WriteLine("Multiple inheritance classes: " + emitter.MixinClassCount + " (mixin refs " + emitter.MixinTotal + ")");
        Console.WriteLine("Warnings:   " + report.Warnings.Count + " | Skipped: " + report.Skipped.Count + " | Notes: " + report.Notes.Count);
        Console.WriteLine("Files:      " + files.Count);
        Console.WriteLine("Report:     " + reportPath);
        return 0;
    }
}
