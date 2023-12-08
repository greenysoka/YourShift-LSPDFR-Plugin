using Rage;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using YourShift.Model;

namespace YourShift.Services
{
    public class SaveAsXaml
    {
        XmlSerializer serializer;
        public string LoacalPath { get; set; }

        public Shift shift;

        public SaveAsXaml() 
        {
            try
            {
                LoacalPath = Directory.GetCurrentDirectory() + @"\plugins\LSPDFR\YourShift.xml";
            }
            catch(Exception ex)
            {
                Game.DisplayNotification(ex.Message);
            }
            
        }

        public void SaveShift(Shift shift)
        {
            TextWriter textWriter = new StreamWriter(LoacalPath);

            serializer = new XmlSerializer(typeof(Shift));
            serializer.Serialize(textWriter, shift);
            textWriter.Close();
        }

        public Shift LoadShift()
        {
            try
            {
                TextReader textReader = new StreamReader(LoacalPath);
                shift = new Shift();

                shift = (Shift)serializer.Deserialize(textReader);
                textReader.Close();

                return shift;
            } catch(Exception ex)
            {
                Game.DisplayNotification(ex.Message);
                Shift shift = new Shift
                {
                    Rank = Rank.Rookie,
                    Shifts = 0
                };
                return shift;
            }
        }
    }
}
