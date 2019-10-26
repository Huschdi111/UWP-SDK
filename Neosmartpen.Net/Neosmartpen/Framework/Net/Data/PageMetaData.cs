using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neosmartpen.Net
{

    /* author:: Lukas Müller
     * e-mail:: lukatoni_mueller@hotmail.com
     * last-update:: 23.10.2019
     * This Code is only guaranteed to be compatible for the neopen models N2 and M1
     * 
     */
    public class PageMetaData
    {
        public PageMetaData(int fileVersion, int pageNumber, int noteID
            , float pageWidth, float pageHeight, long createTimeStamp
            , long modifiedTimeStamp, int dirtyBit, int numberOfStrokes) {
            this.FileVersion = fileVersion;
            this.PageNumber = pageNumber;
            this.NoteId = noteID;
            this.PageWidth = pageWidth;
            this.PageHeight = pageHeight;
            this.CreateTimeStamp = createTimeStamp;
            this.ModifiedTimeStamp = modifiedTimeStamp;
            this.DirtyBit = dirtyBit;
            this.NumberOfStrokes = numberOfStrokes;
        }
        /* Takes a value between 1 and 5*/
        public int FileVersion { get;}
        /* Identifier of the page that this data has been written on */
        public int PageNumber { get;}
        /* Notebook ID*/
        public int NoteId{ get;}
        /* Page Width of the used notebook*/
        public float PageWidth { get;}
        /* Page Height of the used notebook */
        public float PageHeight { get;}
        /* Date of creation saved as a Universal Time*/ 
        public long CreateTimeStamp{ get;}
        /* Date of modification saved as a Universal Time*/
        public long ModifiedTimeStamp { get;}
        /* 1 : data has been tinkert with 0: data is intact as of last modification date*/
        public int DirtyBit { get;}
        /*Number of saved strokes on this page*/
        public int NumberOfStrokes { get;}
    }
}
