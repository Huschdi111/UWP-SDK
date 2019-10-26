using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neosmartpen.Net
{
    public class Page
    {
        public Page(PageMetaData metaData, List<Stroke> strokes) {
            this.MetaData = metaData;
            this.Strokes = strokes;
        }
        /*All Meta Data of the file*/
        public PageMetaData MetaData { get; }
        /*List of all saved Strokes and Dots in the file*/
        public List<Stroke> Strokes { get; }
    }
}