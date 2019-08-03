using System;

namespace DbLight.DataAnnotations
{
    public class TableAttribute : Attribute
    {
        public string Name{ get; set; } 
        public string Database{ get; set; } 

        public TableAttribute(string name){
            Name = name;
        }

        public TableAttribute(){
        }
    }
}