using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RemoteApp
{
    public class XmlHelper
    {
        public static void Save<T>(T obj, string path)
        {
            XmlSerializer xml = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces name = new XmlSerializerNamespaces();
            name.Add("", "");
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                xml.Serialize(writer, obj, name);
                writer.Flush();
                return;
            }
        }

        public static T Read<T>(string filepath)
        {
            XmlSerializer xml = new XmlSerializer(typeof(T));
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Prohibit;

            try
            {
                using (XmlReader reader = XmlReader.Create(filepath, settings))
                {
                    object obj = null;
                    obj = xml.Deserialize(reader);
                    return (T)obj;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return default(T);
            }
        }

        public static T Deserialize<T>(string xmlcontent)
        {
            XmlSerializer xml = new XmlSerializer(typeof(T));

            try
            {
                MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlcontent));
                using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
                {
                    object obj = null;
                    obj = xml.Deserialize(reader);
                    return (T)obj;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return default(T);
            }
        }
    }
}
