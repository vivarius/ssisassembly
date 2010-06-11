using System.Configuration;
using System.IO;
using System.Text;

namespace SSIS.TestLibrary
{
    public class TestClass
    {
        public static string TestMethod(string AppID)
        {
            string returnedString = AppID + " returned value from package execution " + ConfigurationManager.AppSettings["Test"];
            try
            {
                using (Stream stream = new FileStream(@"c:\testSSIS.txt", FileMode.OpenOrCreate))
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(returnedString);

                    stream.Write(buffer, 0, buffer.Length);
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch
            {
            }
            return returnedString;
        }

        public static void TestMethodEx(string AppID, out string returnedString, ref string returnedStringEx)
        {
            returnedString = AppID + " returned value from package execution " + ConfigurationManager.AppSettings["Test"];
            returnedStringEx = returnedString + returnedString;
            try
            {
                using (Stream stream = new FileStream(@"c:\testSSIS2.txt", FileMode.OpenOrCreate))
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(returnedString);

                    stream.Write(buffer, 0, buffer.Length);
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch
            {
            }
        }

        public void TestMethodEx2(string AppID, out string returnedString, ref string returnedStringEx)
        {
            returnedString = AppID + " returned value from package execution " + ConfigurationManager.AppSettings["Test"];
            returnedStringEx = returnedString + returnedString;
            try
            {
                using (Stream stream = new FileStream(@"c:\testSSIS2.txt", FileMode.OpenOrCreate))
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(returnedString);

                    stream.Write(buffer, 0, buffer.Length);
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch
            {
            }
        }

        public static void TestMethodWithoutReturn(string AppID)
        {
            string returnedString = AppID + " returned value from package execution " + ConfigurationManager.AppSettings["Test"];
            try
            {
                using (Stream stream = new FileStream(@"c:\testSSIS1.txt", FileMode.OpenOrCreate))
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(returnedString);

                    stream.Write(buffer, 0, buffer.Length);
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch
            {
            }
        }
    }
}
