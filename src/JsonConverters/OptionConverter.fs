namespace Newtonsoft.Json.Converters

open Microsoft.FSharp.Reflection
open Newtonsoft.Json
open System

type OptionConverter() = 
    inherit JsonConverter()
    
    let isOption (t: Type) = 
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>
    
    override __.WriteJson(writer, value, serializer) = 
        let x = 
            if isNull value then null
            else 
                let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
                fields.[0]  
        serializer.Serialize(writer, x)                
    
    override __.ReadJson(reader, destinationType, _, _) = 
        failwith "not implemented"
    
    override __.CanConvert(objectType) = isOption objectType