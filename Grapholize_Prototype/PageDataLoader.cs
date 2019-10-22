using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Grapholize_Prototype 
{
    public class PageDataLoader
    {
        FileStream filePointer;
        string fileName;
        List<Stroke> strokes;

        public PageDataLoader(string fileName) {
            this.fileName = fileName;
            filePointer = new FileStream(fileName, FileMode.Open);
            if (IsFileValid())
            {
                //TODO Exctract other useful information
                strokes = new List<Stroke>();
                filePointer.Seek(40, SeekOrigin.Begin);
                byte[] numOfStrokesBytes = new byte[4];
                filePointer.Read(numOfStrokesBytes, 0, 4);
                int numOfStrokes = ByteArrayToUIntLSF(numOfStrokesBytes);
                Console.WriteLine(
                    "byte_1: " + numOfStrokesBytes[0] 
                    + " byte_2: " + numOfStrokesBytes[1]
                );
                ParseData(numOfStrokes);
            }
            else { 
                //TODO Throw a parser Exception
                //TODO check that the O(x) time is not overdrawn;
            }
           
        }

        ~PageDataLoader() {
            filePointer.Close();
        }

        private void ParseData(int numOfStrokes) {
           for (int i = 0; i < numOfStrokes; i++) {
                ReadStroke();
           }
        }

        private void ReadStroke() {
            Stroke stroke = new Stroke(1, 1, 1, 1);
            int signalBit = filePointer.ReadByte();
            if (signalBit == 1) {
                JumpAmount(108); //Jump the length of a voice memo
                return;
            }
            JumpAmount(5); //jump over type and thickness
            byte[] integerBuffer = new byte[4];
            byte[] longBuffer = new byte[8];
            //Read Number of dots
            int read4_int = filePointer.Read(integerBuffer, 0, 4);
            int numOfDots = ByteArrayToUIntLSF(integerBuffer);
            //Read Timestamps
            int read8_long = filePointer.Read(longBuffer, 0, 8);
            long timeStamp = ByteArrayToLongLSF(longBuffer);
            //Read Dots
            Console.WriteLine("signal bit : " + signalBit 
                + " numOfDots : " + numOfDots 
                + " timestamp : " + timeStamp
                + " read8_long : " + read8_long
                + " read4_int : " + read4_int);
            ReadDots(stroke, timeStamp, numOfDots);
            strokes.Add(stroke);
        }

        /*Fills Stroke Object with dots while advancing the filePointer*/
        private void ReadDots(Stroke stroke, long lastTimeStamp, int numberOfDots) {
            /* total 13Bytes
             * x float 4Bytes
             * y float 4Bytes
             * pressure 4Bytes
             * timeDiff 1Byte
             */
            byte[] signalBuffer = new byte[4];
            byte[] floatBuffer = new byte[4];
            for (int i = 0; i < numberOfDots; i++) {
                Dot.Builder dotBuilder = new Dot.Builder();
                //TODO normalisierung mit ein rechnens
                int read4_1 = filePointer.Read(floatBuffer, 0, 4);
                float x = ByteArrayToFloat(floatBuffer);
                int read4_2 = filePointer.Read(floatBuffer, 0, 4);
                float y = ByteArrayToFloat(floatBuffer);
                dotBuilder = dotBuilder.coord(x, y);
                //TODO Read force from dotData
                JumpAmount(4); //skip pressure bytes
                /*byte[] pressureBytes = ReadBytesFromArray(dotData, 8, 4);
                float pressure = ByteArrayToFloat(pressureBytes);
                dotBuilder = dotBuilder.force(pressure);*/
                int timeDiff = filePointer.ReadByte();
                dotBuilder = dotBuilder.timestamp(lastTimeStamp + timeDiff);
                stroke.Add(dotBuilder.Build());
                Console.WriteLine("x : " + x
                    + " y : " +  y
                    + " timestamp : " + timeDiff
                    + " read 4_1 : " + read4_1
                    + " read 4_2 : " + read4_2);
            }
            filePointer.Read(signalBuffer, 0, 3);
            signalBuffer[3] = 0;
            //Check signal HEX: 02 00 01 => dec(little endian) 65538
            int signalInteger = ByteArrayToUIntLSF(signalBuffer);
            if (signalInteger == 65538) {
                Console.WriteLine("this is a valid stroke");
            }

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
            if (!System.BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            return System.BitConverter.ToSingle(bytes, 0);
        }
    }
}