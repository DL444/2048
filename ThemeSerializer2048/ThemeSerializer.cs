using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ThemeSerializer2048
{
    public class Theme
    {
        public string Name { get; set; }
        public List<ThemeEntry> Entries { get; set; } = new List<ThemeEntry>();
        public bool Repeat { get; set; }
        public string FontFamily { get; set; }
        public int Weight { get; set; }
        public string Style { get; set; }

        public string GetXmlString()
        {
            return GetXmlString(false);
        }
        public string GetXmlString(bool escape)
        {
            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder))
            {
                writer.WriteStartElement("Theme");
                writer.WriteAttributeString("Name", Name);
                writer.WriteAttributeString("Repeat", Repeat ? "true" : "false");
                writer.WriteAttributeString("FontFamily", FontFamily);
                writer.WriteAttributeString("Weight", Weight.ToString());
                writer.WriteAttributeString("Style", Style);
                writer.WriteStartElement("Entries");
                foreach (var e in Entries)
                {
                    writer.WriteStartElement("Entry");
                    writer.WriteAttributeString("Level", e.Level.ToString());
                    writer.WriteAttributeString("Background", e.BackgroundColor);
                    writer.WriteAttributeString("Foreground", e.ForegroundColor);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            if(escape)
            {
                builder.Replace("\"", "\\\"");
            }
            return builder.ToString();
        }

        public override string ToString()
        {
            return Name;
        }

        public Theme(string xmlString)
        {
            string str = xmlString.Replace("\\\"", "\"");
            try
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(str)))
                {
                    reader.Read();
                    reader.Read();
                    if (reader.AttributeCount < 2)
                    {
                        throw new XmlException();
                    }
                    Name = reader.GetAttribute("Name");
                    Repeat = reader.GetAttribute("Repeat") == "true" ? true : false;
                    string familyStr = reader.GetAttribute("FontFamily");
                    FontFamily = familyStr ?? "Segoe UI";
                    string weightStr = reader.GetAttribute("Weight");
                    Weight = weightStr == null ? 400 : int.Parse(weightStr);
                    string styleStr = reader.GetAttribute("Style");
                    Style = (styleStr == null ? "Normal" : styleStr);
                    reader.Read();
                    while(true)
                    {
                        reader.Read();
                        if(reader.NodeType == XmlNodeType.EndElement)
                        {
                            break;
                        }
                        else
                        {
                            if(reader.AttributeCount != 3) { throw new XmlException(); }
                            ThemeEntry e = new ThemeEntry();
                            try
                            {
                                e.Level = int.Parse(reader.GetAttribute("Level"));
                                e.ForegroundColor = reader.GetAttribute("Foreground");
                                e.BackgroundColor = reader.GetAttribute("Background");
                            }
                            catch (FormatException)
                            {
                                throw new XmlException();
                            }
                            Entries.Add(e);
                        }
                    }
                }
            }
            catch(XmlException)
            {
                throw new ArgumentException("Not a valid theme XML document.", nameof(xmlString));
            }
        }
        public Theme() { }
    }
    public class ThemeEntry
    {
        private string _backgroundColor;
        private string _foregroundColor;

        public int Level { get; set; }
        public string BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (ValidateColor(value) == false) { throw new FormatException(nameof(value)); }
                _backgroundColor = value;
            }
        }
        public string ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                if(ValidateColor(value) == false) { throw new FormatException(nameof(value)); }
                _foregroundColor = value;
            }
        }

        public override string ToString()
        {
            return $"Level: {Level}, Background: {BackgroundColor}, Foreground: {ForegroundColor}";
        }

        bool ValidateColor(string input)
        {
            string str = input.ToUpper();
            if (str.StartsWith("#") && str.Length == 9)
            {
                for(int i = 1; i < 9; i++)
                {
                    if (!((str[i] >= '0' && str[i] <= '9') || (str[i] >= 'A' && str[i] <= 'F'))) { return false; }
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
