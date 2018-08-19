using System;
using ThemeSerializer2048;

namespace TestConsole2048
{
    class Program
    {
        static void Main(string[] args)
        {
            Theme t = new Theme();
            t.Entries.Add(new ThemeEntry() { Level = 1, BackgroundColor = "#AABBCCDD", ForegroundColor = "#BBCCDDEE" });
            t.Entries.Add(new ThemeEntry() { Level = 2, BackgroundColor = "#CCDDEEFF", ForegroundColor = "#ABCDEF12" });
            t.Entries.Add(new ThemeEntry() { Level = -1, BackgroundColor = "#00000000", ForegroundColor = "#01234567" });
            t.Name = "Test";
            t.Repeat = true;
            string s = t.GetXmlString(true);
            Theme ts = new Theme(s);
        }
    }
}
