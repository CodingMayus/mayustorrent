// using System;
// using System.Linq;
// using System.Text;
// using System.Security.Cryptography;
// using System.Collections;
// using System.Collections.Generic;
// using System.Collections.Specialized;
// using System.IO;
// using System.Net;

// namespace BitTorrent
// {
//     public class Torrent
//     {
//         public  String Name {get;private set;}
//         public bool? IsPrivate {get;private set;}
//         public List<FileItem>Files{get;private set;} = new List<FileItem>();
//         public string FileDirectory {get {return (Files.Count > 1) ? Name + Path.DirectorySeparatorChar:"";}}
//         public List<Tracker> Trackers {get;} = new List<Tracker>();
//         public string Comment {get;set;}
//         public string CreatedBy{get;set;}
//         public DateTime CreationDate{get;set;}
//         public Encoding Encoding{get;set;}
//         public int BlockSize {get; private set;}
//         public int PieceSize{get;private set;}
//         public byte[][]PieceHashes {get;private set;}

//     }
// }
// public class FileItem
// {
//     public string Path;
//     public long Size;
//     public long Offset;
//     public string FormattedSize {get {return Torrent.BytesToString(Size);}}

// }
// public class Tracker
// {
//     public event EventHandler<List<IPEndPoint>> PeerListUpdated;
//     public string Address {get;private set;}
//     public Tracker(string address)
//     {
//         Address = address;
//     }

// }