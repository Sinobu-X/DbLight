using System;

namespace DbLight.DataAnnotations
{
    public class ColumnAttribute : Attribute
    {
        public string Name{ get; set; }
        public bool Identity{ get; set; }

        public ColumnAttribute(string name){
            Name = name;
        }

        public ColumnAttribute(){
        }
    }
}