namespace Newtonsoft.Json.Converters

open Microsoft.FSharp.Reflection
open Newtonsoft.Json
open System

type DuConverter() = 
    inherit JsonConverter()
    
    let writeValue (value:obj) (serializer:JsonSerializer, writer : JsonWriter) =
        if value.GetType().IsPrimitive then writer.WriteValue value
        else serializer.Serialize(writer, value)

    let isOption (t: Type) = 
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>

    override __.WriteJson(writer, value, serializer) = 
        let unionType = value.GetType()
        let case, _ = FSharpValue.GetUnionFields(value, unionType)

        writer.WriteValue case.Name
            
    
    override __.ReadJson(reader, destinationType, _, _) = 
        failwith "not implemented"
    
    override __.CanConvert(objectType) = FSharpType.IsUnion objectType && (not (isOption objectType))