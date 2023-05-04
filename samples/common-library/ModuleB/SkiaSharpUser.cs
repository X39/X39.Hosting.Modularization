namespace X39.Hosting.Modularization.Samples.CommonLibrary.ModuleB;

public class SkiaSharpUser
{
    public void Test()
    {
        using var bitmap = new SkiaSharp.SKBitmap(128, 128);
    }
}