using PdfSharpCore.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using to_do_list;

public class CustomFontResolver : IFontResolver
{
    private readonly Dictionary<string, byte[]> fonts = new();

    public string DefaultFontName => "Arial";

    public CustomFontResolver()
    {
        string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts");

        LoadFont("Arial#Regular", Path.Combine(basePath, "ARIAL.TTF"));
        LoadFont("Arial#Bold", Path.Combine(basePath, "ARIALBD.TTF"));
        // Dodaj więcej wariantów jeśli potrzebujesz (Italic, BoldItalic, etc.)
    }

    private void LoadFont(string name, string path)
    {
        if (File.Exists(path))
        {
            fonts[name] = File.ReadAllBytes(path);
        }
        else
        {
            Console.WriteLine($"[FontResolver] {Lang.L("font_missing")} {path}");
        }
    }

    public byte[] GetFont(string faceName)
    {
        if (fonts.TryGetValue(faceName, out var fontData))
            return fontData;

        throw new InvalidOperationException($"Czcionka '{faceName}' nie została znaleziona w resolverze.");
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
        {
            if (isBold)
                return new FontResolverInfo("Arial#Bold");
            else
                return new FontResolverInfo("Arial#Regular");
        }

        // Fallback
        return new FontResolverInfo("Arial#Regular");
    }
}
