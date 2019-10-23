using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Grapholize_Prototype 
{

    /* author:: Lukas Müller
     * e-mail:: lukatoni_mueller@hotmail.com
     * last-update:: 23.10.2019
     * This Code is only guaranteed to be compatible for the neopen models N2 and M1
    */
    public class PageDataLoader
    {
        FileStream filePointer;
        List<Stroke> strokes;

        public PageDataLoader(string fileName) {
            filePointer = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] byteBuffer = new byte[4];
            byte[] byteLongBuffer = new byte[8];
            if (IsFileValid()) { 
                strokes = new List<Stroke>();
                int fileVersion = ReadInteger(byteBuffer);
                JumpAmount(4); // Skip the noteId
                int pageNum = ReadInteger(byteBuffer);
                float page_width = ReadFloat(byteBuffer);
                float page_height = ReadFloat(byteBuffer);
                long createdTimeStamp = ReadLong(byteLongBuffer);
                long modifiedTimeStamp = ReadLong(byteLongBuffer);
                int dirtyBit = filePointer.ReadByte();
                int numOfStrokes = ReadInteger(byteBuffer);
                Console.WriteLine("fileVersion: " + fileVersion 
                    + " pageNum: " + pageNum
                    + " page_width: " + page_width
                    + " page_height: " + page_height
                    + " numOfStrokes: " + numOfStrokes);
                ParseContentBody(numOfStrokes);
            }
            else { 
                //TODO Throw a parser Exception
                //TODO check that the O(x) time is not overdrawn ::
                //TODO parsing error infinite loop occured;
            }
           
        }

        ~PageDataLoader() {
            filePointer.Close();
        }



        private void ParseContentBody(int numOfStrokes) {
           for (int i = 0; i < numOfStrokes; i++) {
                ReadStroke();
           }
           Stroke[] stroky = strokes.ToArray();
           Stroke stroke = stroky[30];
            foreach (Dot dot in stroke) {
                Console.WriteLine("X:: " + dot.X + " Y:: " + dot.Y + " pressure:: " + dot.Force);
            }
        }

        private void ReadStroke() {
            //TODO create a sensible first stroke
            Stroke stroke = new Stroke(1, 1, 1, 1);
            int signalBit = filePointer.ReadByte();
            if (signalBit == 1) {
                JumpAmount(108); //Skip the length of a voice memo
                return;
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
            strokes.Add(stroke);
        }

        /*Fills Stroke Object with dots while advancing the filePointer*/
        private void ReadDots(Stroke stroke, long lastTimeStamp, int numberOfDots) { 
            byte[] signalBuffer = new byte[4];
            byte[] floatBuffer = new byte[4];
            for (int i = 0; i < numberOfDots; i++) {
                Dot.Builder dotBuilder = new Dot.Builder();
                //TODO normalisierung der x , y Koordinaten, überhaupt nötig bei M1 Stift?
                //(x or y dot code from N2) / MAX(width, height)
                float x = ReadFloat(floatBuffer);
                float y = ReadFloat(floatBuffer);
                dotBuilder = dotBuilder.coord(x, y);
                int force = ReadInteger(floatBuffer); //this might be a shitty idea
                dotBuilder = dotBuilder.force(force);
                int timeDiff = filePointer.ReadByte();
                dotBuilder = dotBuilder.timestamp(lastTimeStamp + timeDiff);
                stroke.Add(dotBuilder.Build());
            }
            
            int extraDataNum = filePointer.ReadByte();
            if (extraDataNum == 0) JumpAmount(-1); //Reset the read ahead
            JumpAmount(extraDataNum); // Skip the extra data if it exists
        }

        private bool IsFileValid() {
            //TODO Check other hints that might prove the validity of the file
            byte[] neoSignalWord = new byte[3];
            filePointer.Read(neoSignalWord, 0, 3);
            string signalWord = Encoding.ASCII.GetString(neoSignalWord, 0, neoSignalWord.Length);
            return signalWord.Equals("neo");
        }

        private void JumpAmount(long byteCount) {
            filePointer.Seek(byteCount, SeekOrigin.Current);
        }

        private long ReadLong(byte[] buffer)
        {
            filePointer.Read(buffer, 0, 8);
            return ByteArrayToLongLSF(buffer);
        }

        private int ReadInteger(byte[] buffer)
        {
            filePointer.Read(buffer, 0, 4);
            return ByteArrayToUIntLSF(buffer);
        }

        private float ReadFloat(byte[] buffer) {
            filePointer.Read(buffer, 0, 4);
            return ByteArrayToFloat(buffer);
        }
        //TODO extract all methods below to a utility function
        //TODO eliminate code duplication
        //Geht nach dem Least Significant byte Reihenfolge vor
        private int ByteArrayToUIntLSF(byte[] bytes) {
            int result = 0;
            for(byte i = 0; i < 4; i++) {
                int temp = bytes[i];
                temp = temp << i*8;
                result += temp;
            }
            return result;
        }

        private long ByteArrayToLongLSF(byte[] bytes) {
            long result = 0;
            for (byte i = 0; i < 8; i++) {
                long temp = bytes[i];
                temp = temp << i * 8;
                result += temp;
            }
            return result;
        }

        private float ByteArrayToFloat(byte[] bytes) {
            if (!System.BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return System.BitConverter.ToSingle(bytes, 0);
        }
    }
}