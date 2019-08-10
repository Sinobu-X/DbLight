using System;

namespace DbLight.DataAnnotations
{
    public class NotMappedAttribute: Attribute
    {
        public bool Value { get;}

        public NotMappedAttribute(){
            Value = true;
        }
    }
}