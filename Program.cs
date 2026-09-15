using System;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Collections.Immutable;
using System.Net;

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




        public static byte[] Encode(object obj)
        {
            MemoryStream buffer = new MemoryStream();
            EncodeNextObject(buffer, obj);
            return buffer.ToArray();
        }
        public static void EncodeToFile(object obj, string path)
        {
            File.WriteAllBytes(path, Encode(obj));
        }
        // memoryStream is kind of like malloc 
        private static void EncodeNextObject(MemoryStream buffer, object obj)
        {
            if (obj is byte[])
                EncodeByteArray(buffer, (byte[])obj);

            if (obj is string)
                EncodeString(buffer, (string)obj);
            if (obj is long)
                EncodeNumber(buffer, (long)obj);
            if (obj.GetType() == typeof(List<object>))
            {
                EncodeList(buffer, (List<object>)obj);
            }
            if (obj.GetType() == typeof(Dictionary<string, object>))
            {
                EncodeDictionary(buffer, (Dictionary<string, object>)obj);
            }
            else
                throw new Exception("unable to encode type " + obj.GetType());
        }

        private static void EncodeNumber(MemoryStream buffer, long input)
        {
            buffer.Append(NumberStart);
            buffer.Append(Encoding.UTF8.GetBytes(Convert.ToString(input)));
            buffer.Append(NumberEnd);
        }
        private static void EncodeByteArray(MemoryStream buffer, byte[] array)
        {
            buffer.Append(Encoding.UTF8.GetBytes(Convert.ToString(array.Length)));
            buffer.Append(ByteArrayDivider);
            buffer.Append(array);
        }
        private static void EncodeString(MemoryStream buffer, string input)
        {
            EncodeByteArray(buffer, Encoding.UTF8.GetBytes(input));
        }
        private static void EncodeList(MemoryStream buffer, List<object> input)
        {
            buffer.Append(ListStart);
            foreach (var item in input)
            {
                EncodeNextObject(buffer, item);
            }
            buffer.Append(ListEnd);
        }
        private static void EncodeDictionary(MemoryStream buffer, Dictionary<string, object> dict)
        {
            buffer.Append(DictionaryStart);
            // because dictionaries orders cannot be guaranteed, we convert to the list to sort then append to the memorystream!
            var sortedKeys = dict.Keys.ToList().OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));
            foreach (var key in sortedKeys)
            {
                EncodeString(buffer, key);
                EncodeNextObject(buffer, dict[key]);
            }

            buffer.Append(DictionaryEnd);
        }



    }
    
}







    public class Torrent
    {
        public  String Name {get;private set;}
        public bool? IsPrivate {get;private set;}
        public List<FileItem>Files{get;private set;} = new List<FileItem>();
        public string FileDirectory {get {return (Files.Count > 1) ? Name + Path.DirectorySeparatorChar:"";}}
        public List<Tracker> Trackers {get;} = new List<Tracker>();
        public string Comment {get;set;}
        public string CreatedBy{get;set;}
        public DateTime CreationDate{get;set;}
        public Encoding Encoding{get;set;}
        public int BlockSize {get; private set;}
        public int PieceSize{get;private set;}
        public long TotalSize{get {return Files.Sum(x=>x.Size);}}
        public byte[][]PieceHashes {get;private set;}
        public byte[] Infohash{get;set;} = new byte[20];
        public string HexStringInfohash{get{return String.Join("", this.Infohash.Select(x=>x.ToString("x2")));}}
        public string UrlSafeStringInfohash {get { return Encoding.UTF8.GetString(WebUtility.UrlEncodeToBytes(this.Infohash, 0,20));}}
        
    }

public class FileItem
{
    public string Path;
    public long Size;
    public long Offset;
    public string FormattedSize {get {return Torrent.BytesToString(Size);}}

}
public class Tracker
{
    public event EventHandler<List<IPEndPoint>> PeerListUpdated;
    public string Address {get;private set;}
    public Tracker(string address)
    {
        Address = address;
    }

}








//extend the MemoryStream class to make writing byte arrays a little bit tidier
public static class MemoryStreamExtensions
{
    public static void Append(this MemoryStream stream, byte value)
    {
        stream.Append(new[] { value });
    }
    public static void Append(this MemoryStream stream, byte[] values)
    {
        stream.Write(values, 0, values.Length);
    }



}
