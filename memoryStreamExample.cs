// Creates a stream whose abcking store is memory
// public class MemoryStream: System.IO.Strea

using System;
using System.IO;
using System.Text;

class MemStream
{
    
    static void Main()
    {
        int count;
        byte[] byteArray;
        char[] charArray;
        UnicodeEncoding uniEncoding = new UnicodeEncoding();
        //Creates teh data to write tot he stream
        byte[] firstString = uniEncoding.GetBytes("Invalid file path characters are: ");
        byte[] secondString = uniEncoding.GetBytes(Path.GetInvalidFileNameChars());

        using(MemoryStream memStream = new MemoryStream(100))
        {   //wRITE THE FIRST STRING TO THE STREAM
        memStream.Write(firstString,0,firstString.Length);
            //Write the second string to the stream, byte by byte. 
            count =0;
            while(count< secondString.Length)
            {
                memStream.WriteByte(secondString[count++]);
            }

            //write the stream properties to the console.

            Console.WriteLine(
                "Capacity={0}, Length={1}, Position ={2}\n",
                memStream.Capacity.ToString(),
                memStream.Length.ToString(),
                memStream.Position.ToString());
                // set or retrieve position with seek.
                memStream.Seek(0,SeekOrigin.Begin);

                //Read the first 20 bytes from the stream
                byteArray = new byte[memStream.Length];
                count = memStream.Read(byteArray,0,20);

            while (count < memStream.Length)
            {
                byteArray[count++]=(byte)memStream.ReadByte();
            }

            //Decode the byte array into char array
            // Write it to console
            charArray = new char[uniEncoding.GetCharCount(byteArray,0,count)];
            uniEncoding.GetDecoder().GetChars(byteArray,0,count, charArray,0);
            Console.WriteLine(charArray);
        }
    }
}