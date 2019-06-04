namespace Newtonsoft.Json.Converters

open Microsoft.FSharp.Reflection
open Newtonsoft.Json
open System

type TimeSpanConverter() = 
    inherit JsonConverter()
    
    override __.WriteJson(writer, value, serializer) = 
        
        serializer.Serialize(writer, (System.Xml.XmlConvert.ToString (unbox<TimeSpan> value)))                
    
    override __.ReadJson(reader, destinationType, _, _) = 
        failwith "not implemented"
    
    override __.CanConvert(objectType) = objectType = typedefof<TimeSpan>