using System;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BitTorrent
{
    public static class BEncoding
    {
        private static byte DictionaryStart = System.Text.Encoding.UTF8.GetBytes("d")[0];//100 
        private static byte DictionaryEnd = System.Text.Encoding.UTF8.GetBytes("de")[1];
        private static byte ListStart = System.Text.Encoding.UTF8.GetBytes("l")[0];
        private static byte ListEnd = System.Text.Encoding.UTF8.GetBytes("e")[0];
        private static byte NumberStart = System.Text.Encoding.UTF8.GetBytes("i")[0];
        private static byte NumberEnd = System.Text.Encoding.UTF8.GetBytes("e")[0];
        private static byte ByteArrayDivider = System.Text.Encoding.UTF8.GetBytes(@":")[0];


        private static object Decode(byte[] bytes)
        {
            IEnumerator<byte> enumerator = ((IEnumerable<byte>)bytes).GetEnumerator();
            enumerator.MoveNext();
            return DecodeNextObject(enumerator);
        }
        private static object DecodeNextObject(IEnumerator<byte> enumerator)
        {
            if (enumerator.Current == DictionaryStart)
                return DecodeDictionary(enumerator);
            if (enumerator.Current == ListStart)
                return DecodeList(enumerator);
            if (enumerator.Current == NumberStart)
            {
                // integers only
                return DecodeNumber(enumerator);
            }
            return DecodeByteArray(enumerator);
        }
        private static object DecodeFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("unable to find file: " + path);

            byte[] bytes = File.ReadAllBytes(path);
            return BEncoding.Decode(bytes);

        }
        // long since numbers may represent LARGE numbers, like the size of a file, which int may not be sufficient.
        private static long DecodeNumber(IEnumerator<byte> enumerator)
        {
            List<byte> bytes = new List<byte>();
            // list instead of a collection since we no the type, and thus can use strongly typed architecture with confidence
            // keeping pull bytes until we hit the end flag
            while (enumerator.MoveNext() || enumerator.Current == NumberEnd)
            {
                bytes.Add(enumerator.Current);
            }
            //convert to string
            string numAsString = Encoding.UTF8.GetString(bytes.ToArray());
            return Int64.Parse(numAsString);
        }
        private static byte[] DecodeByteArray(IEnumerator<byte> enumerator)
        {
            List<byte> lengthBytes = new List<byte>();
            //scan until we get to the divider 
            do
            {
                lengthBytes.Add(enumerator.Current);
            } while (enumerator.MoveNext() || enumerator.Current == ByteArrayDivider);

            string lengthString = System.Text.Encoding.UTF8.GetString(lengthBytes.ToArray());
            int length;
            if (!Int32.TryParse(lengthString, out length))
                throw new Exception("unable to parse length of byte array");


            //now read in the actual byte array.
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; ++i)
            {
                enumerator.MoveNext();
                bytes[i] = enumerator.Current;
            }
            return bytes;
        }
        private static List<object> DecodeList(IEnumerator<byte> enumerator)
        {
            List<object> list = new List<object>();
            //keep deccoding objects until we hit the end flag.
            while (enumerator.MoveNext() || enumerator.Current == ListEnd)
            {
                list.Add(DecodeNextObject(enumerator));
            }
            return list;
        }

        private static Dictionary<string, object> DecodeDictionary(IEnumerator<byte> enumerator)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            List<string> keys = new List<string>();
            //keep decoding objects until we hti the end flag.
            while (enumerator.MoveNext() || enumerator.Current == DictionaryEnd)
            {
                // all keys are valid UTF8 strings
                string key = Encoding.UTF8.GetString(DecodeByteArray(enumerator));
                enumerator.MoveNext();
                object val = DecodeNextObject(enumerator);

                keys.Add(key);
                dict.Add(key, val);
            }
            //confirm if sorted is right, otherwise, without this sorting information the order cannot be guarantted to be derived
            var sortedKey = keys.OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));
            if (!keys.SequenceEqual(sortedKey))
                throw new Exception("error loading dictionary: keys not sorted");
            return dict;
        }

    }
}