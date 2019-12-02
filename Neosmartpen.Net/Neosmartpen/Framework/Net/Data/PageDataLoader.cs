using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Neosmartpen.Net
{

    /* author:: Lukas Müller, Jonas Arab
     * e-mail:: lukatoni_mueller@hotmail.com
     * last-update:: 23.10.2019
     * This Code is only guaranteed to be compatible for the neopen models N2 and M1
    */
    public static class PageDataLoader
    {
        private static FileStream filePointer;
        //private List<Stroke> strokes;
        //public Page Page { get; private set; }

        public static Page LoadPage(string fileName) {
            filePointer = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            if (IsFileValid())
            {
                //strokes = new List<Stroke>();

                PageMetaData metaData = ReadMetaData();

                var strokes = ParseContentBody(metaData.NumberOfStrokes);

                filePointer.Dispose();
                filePointer = null;
                return new Page(metaData, strokes);
            }
            else {
                //TODO Throw a parser Exception
                //TODO check that the O(x) time is not overdrawn ::
                //TODO parsing error infinite loop occured;
                throw new FileLoadException("File could not be read.");
            }

        }

        private static PageMetaData ReadMetaData() {
            byte[] byteBuffer = new byte[4];
            byte[] byteLongBuffer = new byte[8];
            int fileVersion = ReadInteger(byteBuffer);
            int noteId = ReadInteger(byteBuffer);
            int pageNum = ReadInteger(byteBuffer);
            float pageWidth = ReadFloat(byteBuffer);
            float pageHeight = ReadFloat(byteBuffer);
            long createdTimeStamp = ReadLong(byteLongBuffer);
            long modifiedTimeStamp = ReadLong(byteLongBuffer);
            int dirtyBit = filePointer.ReadByte();
            int numOfStrokes = ReadInteger(byteBuffer);

           return new PageMetaData(fileVersion, pageNum, noteId
                , pageWidth, pageHeight, createdTimeStamp
                , modifiedTimeStamp, dirtyBit, numOfStrokes);

        }

        private static List<Stroke> ParseContentBody(int numOfStrokes) {
            var result = new List<Stroke>();
            for (int i = 0; i < numOfStrokes; i++) {
                result.Add(ReadStroke());
           }
            return result;
           //Stroke[] stroky = strokes.ToArray();
           //Stroke stroke = stroky[30];
           // foreach (Dot dot in stroke) {
           //     //Console.WriteLine("X:: " + dot.X + " Y:: " + dot.Y + " pressure:: " + dot.Force);
           // }
        }

        private static Stroke ReadStroke() {
            //TODO create a sensible first stroke
            Stroke stroke = new Stroke(1, 1, 1, 1);
            int signalBit = filePointer.ReadByte();
            if (signalBit == 1) {
                JumpAmount(108); //Skip the length of a voice memo
            }
            JumpAmount(5); //Skip over type and thickness
            byte[] integerBuffer = new byte[4];
            byte[] longBuffer = new byte[8];
            //Read Number of Dots
            int numOfDots = ReadInteger(integerBuffer);
            //Read Timestamps
            long timeStamp = ReadLong(longBuffer);
            //Read Dots
            ReadDots(stroke, timeStamp, numOfDots);
            //strokes.Add(stroke);
            return stroke;
        }

        /*Fills Stroke Object with dots while advancing the filePointer*/
        private static void ReadDots(Stroke stroke, long lastTimeStamp, int numberOfDots) { 
            byte[] signalBuffer = new byte[4];
            byte[] floatBuffer = new byte[4];
            for (int i = 0; i < numberOfDots; i++) {
                Dot.Builder dotBuilder = new Dot.Builder();
                //TODO normalisierung der x , y Koordinaten, überhaupt nötig bei M1 Stift?
                //(x or y dot code from N2) / MAX(width, height)
                float x = ReadFloat(floatBuffer);
                float y = ReadFloat(floatBuffer);
                dotBuilder = dotBuilder.coord(x, y);
                int force = ReadInteger(floatBuffer); //this might be a shitty idea float -> int
                dotBuilder = dotBuilder.force(force);
                int timeDiff = filePointer.ReadByte();
                dotBuilder = dotBuilder.timestamp(lastTimeStamp + timeDiff);
                stroke.Add(dotBuilder.Build());
                
            }
            
            int extraDataNum = filePointer.ReadByte();
            if (extraDataNum == 0) JumpAmount(-1); //Reset the read ahead
            JumpAmount(extraDataNum); // Skip the extra data if it exists
        }

        private static bool IsFileValid() {
            //TODO Check other hints that might prove the validity of the file
            byte[] neoSignalWord = new byte[3];
            filePointer.Read(neoSignalWord, 0, 3);
            string signalWord = Encoding.ASCII.GetString(neoSignalWord, 0, neoSignalWord.Length);
            return signalWord.Equals("neo");
        }

        private static void JumpAmount(long byteCount) {
            filePointer.Seek(byteCount, SeekOrigin.Current);
        }

        private static long ReadLong(byte[] buffer)
        {
            filePointer.Read(buffer, 0, 8);
            return ByteArrayToLongLSF(buffer);
        }

        private static int ReadInteger(byte[] buffer)
        {
            filePointer.Read(buffer, 0, 4);
            return ByteArrayToUIntLSF(buffer);
        }

        private static float ReadFloat(byte[] buffer) {
            filePointer.Read(buffer, 0, 4);
            return ByteArrayToFloat(buffer);
        }
        //TODO extract all methods below to a utility function
        //TODO eliminate code duplication
        //Geht nach dem Least Significant byte Reihenfolge vor
        private static int ByteArrayToUIntLSF(byte[] bytes) {
            int result = 0;
            for(byte i = 0; i < 4; i++) {
                int temp = bytes[i];
                temp = temp << i*8;
                result += temp;
            }
            return result;
        }

        private static long ByteArrayToLongLSF(byte[] bytes) {
            long result = 0;
            for (byte i = 0; i < 8; i++) {
                long temp = bytes[i];
                temp = temp << i * 8;
                result += temp;
            }
            return result;
        }

        private static float ByteArrayToFloat(byte[] bytes) {
            if (!System.BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return System.BitConverter.ToSingle(bytes, 0);
        }
    }
}