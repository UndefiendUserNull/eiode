using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EIODE.Utils;

public static class FilesUtils
{
    /// <summary>
    /// Reads and cleans given file path
    /// </summary>
    public static List<string> ReadFile(string path)
    {
        List<string> result = [];
        try
        {
            var lines = File.ReadAllLines(path);
            result = lines.Select(line => line.Trim()).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        }
        catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException || e is IOException)
        {
            {
                GD.PushError($"Error while reading {path} : {e}");
                throw;
            }
        }

        return result;
    }
}
