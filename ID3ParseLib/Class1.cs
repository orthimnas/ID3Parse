using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ID3ParseLib
{
    public class MP3File
    {
        private readonly System.IO.FileStream _fs = null;
        private readonly string strPathToFile = string.Empty;
        private readonly ID3v1 _ID3v1;
        private readonly ID3v2 _ID3v2;

        public MP3File(string strPathToFile)
        {
            //Throws an exception if file doesn't exist
            using (FileStream fs = new FileStream(strPathToFile, FileMode.Open, FileAccess.Read))
            {
                this._ID3v1 = new ID3v1(fs);
                this._ID3v2 = new ID3v2(fs);
            }
        }

        public ID3v1 ID3v1 { get { return this._ID3v1; } }
        public ID3v2 ID3v2 { get { return this._ID3v2; } }
    }

    public enum GenreType : int
    {
        Unknown = -1,
        Blues = 0,
        Classic_Rock,
        Country,
        Dance,
        Disco,
        Funk,
        Grunge,
        Hip_Hop,
        Jazz,
        Metal,
        New_Age,
        Oldies,
        Other,
        Pop,
        R_and_B,
        Rap,
        Reggae,
        Rock,
        Techno,
        Industrial,
        Alternative,
        Ska,
        Death_Metal,
        Pranks,
        Soundtrack,
        Euro_Techno,
        Ambient,
        Trip_Hop,
        Vocal,
        Jazz_Funk,
        Fusion,
        Trance,
        Classical,
        Instrumental,
        Acid,
        House,
        Game,
        Sound_Clip,
        Gospel,
        Noise,
        AlternRock,
        Bass,
        Soul,
        Punk,
        Space,
        Meditative,
        Instrumental_Pop,
        Instrumental_Rock,
        Ethnic,
        Gothic,
        Darkwave,
        Techno_Industrial,
        Electronic,
        Pop_Folk,
        Eurodance,
        Dream,
        Southern_Rock,
        Comedy,
        Cult,
        Gangsta,
        Top_40,
        Christian_Rap,
        Pop_Funk,
        Jungle,
        Native_American,
        Cabaret,
        New_Wave,
        Psychadelic,
        Rave,
        Showtunes,
        Trailer,
        Lo_Fi,
        Tribal,
        Acid_Punk,
        Acid_Jazz,
        Polka,
        Retro,
        Musical,
        Rock_and_Roll,
        Hard_Rock
    }

    internal static class Helper
    {
        internal static string ReadFromStream(FileStream fs, int nLength)
        {
            byte[] bReadBuffer = new byte[nLength];
            fs.Read(bReadBuffer, 0, nLength);
            string strValue = System.Text.Encoding.ASCII.GetString(bReadBuffer);
            if (strValue.IndexOf('\0') == -1)
                return strValue;
            else
                return strValue.Substring(0, strValue.IndexOf('\0'));
        }

        internal static UInt32 ReadUInt64FromStream(FileStream fs, int nLength)
        {
            byte[] buf = new byte[nLength];
            byte[] bufReversed = new byte[4];
            fs.Read(buf, 0, nLength);
            buf.CopyTo(bufReversed, 4 - buf.Length);
            Array.Reverse(bufReversed);
            return BitConverter.ToUInt32(bufReversed, 0);
        }
    }

    public class ID3v1
    {
        private string _title;
        private string _artist;
        private string _album;
        private int _year;
        private string _comment;
        private int _track;
        private GenreType _genre;
        private int _genreNumber;

        internal ID3v1(FileStream fSource)
        {
            //Move to end of file
            fSource.Seek(-128, SeekOrigin.End);

            //Begin parsing
            if (Helper.ReadFromStream(fSource, 3) == "TAG")
            {
                //Valid ID3 tag starting header found, parse out remaining data.
                this._title = Helper.ReadFromStream(fSource, 30).Trim();
                this._artist = Helper.ReadFromStream(fSource, 30).Trim();
                this._album = Helper.ReadFromStream(fSource, 30).Trim();
                int.TryParse(Helper.ReadFromStream(fSource, 4), out this._year);
                this._comment = Helper.ReadFromStream(fSource, 30).Trim();
                if (this._comment.Length <= 28)
                {
                    //Room for a track number to be stored
                    int.TryParse(Helper.ReadFromStream(fSource, 2), out _track);
                }

                //If something goes wrong parsing the Genre Type, assign it an Unknown value.
                try
                {
                    this._genreNumber = int.Parse(Helper.ReadFromStream(fSource, 1));
                    this._genre = (GenreType)this._genreNumber;
                }
                catch
                {
                    this._genreNumber = -1;
                    this._genre = GenreType.Unknown;
                }
            }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        public string Artist
        {
            get { return _artist; }
            set { _artist = value; }
        }
        public string Album
        {
            get { return _album; }
            set { _album = value; }
        }
        public int Year
        {
            get { return _year; }
            set { _year = value; }
        }
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }
        public int Track
        {
            get { return _track; }
            set { _track = value; }
        }
        public GenreType Genre
        {
            get { return _genre; }
            set { _genre = value; }
        }
        public int GenreNumber
        {
            get { return _genreNumber; }
            set { _genreNumber = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Title\t{0}", this._title));
            sb.AppendLine(string.Format("Artist\t{0}", this._artist));
            sb.AppendLine(string.Format("Year\t{0} ({1} years old)", this._year, this._year > 0 ? ((DateTime.Now - new DateTime(this._year, 1, 1)).TotalDays / 365.0f).ToString("F1") : "--"));
            sb.AppendLine(string.Format("Comment\t{0}", this._comment));
            sb.AppendLine(string.Format("Track\t{0}", this._track));
            sb.AppendLine(string.Format("Genre\t{0}", Enum.GetName(typeof(GenreType), this._genre)));
            
            return sb.ToString();
        }
    }

    public class Frame
    {
        private string _name;
        private string _iD;
        private string _value;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string ID
        {
            get { return _iD; }
            set { _iD = value; }
        }
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        internal Frame(FileStream fs)
        {
            //Assumes that File Stream is indexed to frame ID start
            this._iD = Helper.ReadFromStream(fs, 4);
            
            //Get size
            UInt32 size = Helper.ReadUInt64FromStream(fs, 4);

            //Skip flags & consume 1st character
            fs.Seek(3, SeekOrigin.Current);

            //Read data into value
            this._value = Helper.ReadFromStream(fs, (int)size - 1);
        }
    }

    public class ID3v2
    {
        private List<Frame> _frames = new List<Frame>();
        public Frame[] Frames
        {
            get { return _frames.ToArray(); }
        }

        internal ID3v2(FileStream fSource)
        {
            //Move to start of file
            fSource.Seek(0, SeekOrigin.Begin);

            //Begin parsing

            if (Helper.ReadFromStream(fSource, 3) == "ID3" &&
                Helper.ReadUInt64FromStream(fSource, 1) == 3)   //Version is 2.3
            {
                //Valid ID3 tag starting header found, parse out remaining data.

                //Move to the 10th byte (end of header)
                fSource.Seek(10, SeekOrigin.Begin);
                
                while (true)
                {
                    //Check if next 4 characters are [A-Z] indicating another tag
                    string strNextTag = Helper.ReadFromStream(fSource, 4);
                    if (strNextTag.Length != 4)
                        break;

                    foreach (char c in strNextTag)
                    {
                        if (c < 'A' || c > 'Z')
                        {
                            //Not a valid tag, must be done parsing
                            break;
                        }
                    }

                    fSource.Seek(-4, SeekOrigin.Current);
                    this._frames.Add(new Frame(fSource));
                }
                
                //Fill in known tag elements
                foreach (Frame f in this._frames)
                {
                    if (f.ID == "TALB")
                        f.Name = "Album";
                    else if (f.ID == "TIT2")
                        f.Name = "Title";
                    else if (f.ID == "TPE1")
                        f.Name = "Artist";
                    else if (f.ID == "TYER" || f.ID == "TDRC")
                        f.Name = "Year";
                    else if (f.ID == "TRCK")
                        f.Name = "Track";
                    else if (f.ID == "TLEN")
                        f.Name = "Length";
                    else if (f.ID == "TCON")
                        f.Name = "Genre";
                }
            }
        }
    }
}
