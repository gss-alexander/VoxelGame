using System.Runtime.InteropServices;
using FreeTypeSharp;
using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;

namespace Client.UI.Text;

public static class FontLoader
{
    private static unsafe FT_FaceRec_* _currentFace;
    private static unsafe FT_LibraryRec_* _library;
    
    public static void LoadFace(string fontPath)
    {
        unsafe
        {
            FT_LibraryRec_* lib;
            FT_FaceRec_* face;
            var error = FT_Init_FreeType(&lib);

            error = FT_New_Face(lib, (byte*)Marshal.StringToHGlobalAnsi(fontPath), 0, &face);
            error = FT_Set_Char_Size(face, 0, 16 * 64, 300, 300);
            var glyphIndex = FT_Get_Char_Index(face, 'F');
            error = FT_Load_Glyph(face, glyphIndex, FT_LOAD_DEFAULT);
            error = FT_Render_Glyph(face->glyph, FT_RENDER_MODE_NORMAL);

            _library = lib;
            _currentFace = face;
            
            Console.WriteLine($"[FontLoader]: Loaded face from path {fontPath}");
        }
    }

    public static unsafe void LoadChar(char c)
    {
        FT_Load_Char(_currentFace, c, FT_LOAD_RENDER);
    }

    public static void ClearFreeTypeResources()
    {
        unsafe
        {
            FT_Done_Face(_currentFace);
            FT_Done_FreeType(_library);
        }
    }

    public static unsafe uint LoadedCharWidth => _currentFace->glyph->bitmap.width;
    public static unsafe uint LoadedCharHeight => _currentFace->glyph->bitmap.rows;
    public static unsafe int LoadedCharLeft => _currentFace->glyph->bitmap_left;
    public static unsafe int LoadedCharTop => _currentFace->glyph->bitmap_top;
    public static unsafe IntPtr LoadedCharAdvanceX => _currentFace->glyph->advance.x;
    public static unsafe byte* LoadedCharBuffer => _currentFace->glyph->bitmap.buffer;
}