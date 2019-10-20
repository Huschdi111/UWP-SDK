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
            //TODO check if the file is a neo file (identifier at the beginning)
            byte[] neoSignalWord = new byte[3];
            filePointer.Read(neoSignalWord, 0, 3);
            string signalWord = Encoding.UTF8.GetString(neoSignalWord, 0, neoSignalWord.Length);
            if(signalWord.Equals("neo")) {
                Console.WriteLine("this is amazing");
            }
            filePointer.Seek(40, SeekOrigin.Begin);
        }

        ~PageDataLoader() {
            filePointer.Close();
        }


        private void ParseData() {
            uint numOfStrokes = 0;
            for (uint i = 0; i < numOfStrokes; i++) {
                Stroke stroke = ReadStroke();
                strokes.Add(stroke);
                JumpAmount(18);
            }
        }

        private Stroke ReadStroke() {
            Stroke stroke = new Stroke(1, 1, 1, 1);
            byte[] dotData = new byte[18];
            filePointer.Read(dotData, 0, 18);
            return stroke;
        }

        /*Fills Stroke Object with dots while advancing the filePointer*/
        private void ReadDots(Stroke stroke, long lastTimeStamp, long numberOfDots) {
            /* total 13Bytes
             * x float 4Bytes
             * y float 4Bytes
             * pressure 4Bytes
             * timeDiff 1Byte
             */
            byte timeDiff = 1;
            byte[] dotData = new byte[13];
            for (long i = 0; i < numberOfDots; i++) {
                //TODO read 13Bytes
                filePointer.Read(dotData, 0, 13);
                Dot.Builder dotBuilder = new Dot.Builder();
                //TODO Read floats from dotData
                dotBuilder = dotBuilder.coord(1, 1);
                //TODO Read force from dotData
                dotBuilder = dotBuilder.force(1);
                //TODO Read timestamps from dotData
                dotBuilder = dotBuilder.timestamp(lastTimeStamp + timeDiff);
                stroke.Add(dotBuilder.Build());
                JumpAmount(13);
            }
        }

        private void JumpAmount(long byteCount) {
            filePointer.Seek(byteCount,SeekOrigin.Current);
        }
        
    }
}
