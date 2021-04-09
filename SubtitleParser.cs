// File:        SubtitleParser.cs
// Programmer:  Emily Goodwin
// Date:        2021 March 28
// Project:     Video Player w/ auto-scrolling subtitles
// Description: Contains the .srt file parser, parse into a sub title item which has beginning and end time with subtitle.
//              Also combines multiple lines into a single line if the end time = a beginning time

using System.Collections.Generic;
using System.IO;
using System;

namespace VideoPlayer

{   // Class:   SubtitleItem
    // Desc:    Models a sub title item with beginning time, end, and contents
    class SubtitleItem
    {
        // Method: SubtitleItem
        // Params: 
        // Desc: constructor
        public SubtitleItem()
        {

        }

        // Method: SubtitleItem
        // Params: 
        //  int sequence        - sequence number of the subtitle 
        //  string timeBegin    - the beginning time of the subtitle
        //  string timeEnd      - the end time of the subtitle
        //  string subtitle     - the subtitle content
        // Desc: constructor
        public SubtitleItem(int sequence, string timeBegin, string timeEnd, string subtitle)
        {
            Sequence = sequence;
            //ensure that the milliseconds seperator is a '.' since en-us requires this
            timeBegin = timeBegin.Replace(",", ".");
            timeEnd = timeEnd.Replace(",", ".");
            timeStampBegin = TimeSpan.Parse(timeBegin);
            timeStampEnd = TimeSpan.Parse(timeEnd);
            Subtitle = subtitle;
        }

        // Method: TimeStamp
        // Params: 
        // Desc: Getter for time stamp, formats as hh:mm:ss - hh:mm:ss
        public string TimeStamp {
            get
            {
                return (timeStampBegin.ToString(@"hh\:mm\:ss") + " - " + timeStampEnd.ToString(@"hh\:mm\:ss"));
            }
        }

        // Method: timeStampBegin
        // Params: 
        // Desc: Getter/setter for beginning time 
        private TimeSpan timeStampBegin { get; set; }

        // Method: timeStampBegin
        // Params: 
        // Desc: Getter/setter for end time
        private TimeSpan timeStampEnd { get; set; }

        // Method: GetBeginningTimeStamp
        // Params: 
        // Desc: return the beginning time stamp
        public TimeSpan GetBeginningTimeStamp()
        {
            return timeStampBegin;
        }

        // Method: GetEndingTimeStamp
        // Params: 
        // Desc: return the ending time stamp
        public TimeSpan GetEndingTimeStamp()
        {
            return timeStampEnd;
        }

        // Method: SetEndingTimeStamp
        // Params: 
        //  TimeSpan newTimeStamp - the new time stamp to set the time stamp to
        // Desc: sets the ending time stamp with a new timespan
        public void SetEndingTimeStamp(TimeSpan newTimeStamp)
        {
            timeStampEnd = newTimeStamp;
        }

        // Method: Subtitle
        // Params: 
        // Desc: Getter/setter for beginning time 
        public string Subtitle { get; set; }

        // Method: Sequence
        // Params: 
        // Desc: Getter/setter for beginning time 
        public int Sequence { get; set; }
    }

    // Class:   SRT_Parser
    // Desc:    Parses the SRT file.
    class SRT_Parser
    {
        Dictionary<int, SubtitleItem> lastParsedList = new Dictionary<int, SubtitleItem>();
        string lastFileName = "";

        // Method: SubtitleItem
        // Params: 
        // Desc: constructor
        public SRT_Parser()
        {

        }

        // Method: SubtitleItem
        // Params: 
        //  string filePath - the file path to parse
        // Desc: constructor
        public SRT_Parser(string filePath)
        {
            parseFile(filePath);
        }

        // Method: ParseFile
        // Params: 
        //  string filePath - the file to parse
        // Desc: parse the .srt file into a sub title item, combine lines that have a start time that equals an end time
        // Returns: 
        //  Dictionary<int,SubtitleItem> - the parsed dictionary of subtitles with time stamps
        public Dictionary<int, SubtitleItem> ParseFile(string filePath)
        {
            parseFile(filePath);
            return lastParsedList;
        }

        // Method: ParseFile
        // Params: 
        //  string filePath - the file to parse
        // Desc: parse the .srt file into a sub title item, combine lines that have a start time that equals an end time
        private void parseFile(string filePath)
        {
            if (filePath == lastFileName)
            {
                return;
            }

            lastParsedList = new Dictionary<int, SubtitleItem>();
            using (StreamReader fileSource = new StreamReader(filePath))
            {
                List<SubtitleItem> list = new List<SubtitleItem>();
                SubtitleItem prevSubtitle = null;
                while (!fileSource.EndOfStream)
                {
                    int sequenceNumber = int.Parse(fileSource.ReadLine());
                    string timestampStr = fileSource.ReadLine();
                    string[] timeStampSplitStr = timestampStr.Split(new string[] { "-->" }, System.StringSplitOptions.None);
                    string captions = "";
                    string lastLine = "";
                    
                    //get current subtitle, read until empty line
                    do
                    {
                        lastLine = fileSource.ReadLine();
                        if (string.IsNullOrWhiteSpace(lastLine))
                        {
                            break;
                        }
                        captions += lastLine + " ";
                    } while (!string.IsNullOrEmpty(lastLine));
                    captions = captions.Replace("\n", "");

                    SubtitleItem subtitle = new SubtitleItem(sequenceNumber, timeStampSplitStr[0], timeStampSplitStr[1], captions);

                    // combine timestamps if they are continuous
                    if (prevSubtitle?.GetEndingTimeStamp() == subtitle.GetBeginningTimeStamp())
                    {
                        list[list.Count-1].Subtitle += captions;
                        list[list.Count - 1].SetEndingTimeStamp(subtitle.GetEndingTimeStamp());
                    }
                    else
                    {
                        list.Add(subtitle);
                    }
                    prevSubtitle = subtitle;
                }

                // add the subtitles to the list 
                foreach(SubtitleItem item in list)
                {
                    lastParsedList.Add((int)item.GetBeginningTimeStamp().TotalSeconds, item);
                }
            }

            lastFileName = filePath;
        }
    }
}
