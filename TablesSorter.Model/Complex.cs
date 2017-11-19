using System;

namespace TablesSorter.Model
{
    public class Complex: System.IComparable
    {
        public string ValueForSort { get; set; }

        public string Content { get; set; }

        public Complex(string content)
        {
            Content = content;
            ValueForSort = string.Empty;
        }

        public Complex()
        {
        }

        public int CompareTo(object obj)
        {
            var second = obj as Complex;
            var result = 0;

            if (second == null)
                result = 0;

            if (second.Content.Trim() == this.Content.Trim())
                result = 1;

            return result;
        
        }
    }
}
