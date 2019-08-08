namespace Bluefin

open System.Net.Http
open Newtonsoft.Json
open System.Text
open System.Net.Http.Headers
open Newtonsoft.Json.Converters
open System.Threading.Tasks
open Core

module Http = 

    let send (client:HttpClient) (method:HttpMethod) (url:string) (accessToken:string option) (payload:System.Object option)  =
        let message = new HttpRequestMessage (method, url)

        let settings = JsonSerializerSettings()
        settings.Converters.Add(StringEnumConverter())
        settings.Converters.Add(DuConverter())
        settings.Converters.Add(OptionConverter())
        settings.Converters.Add(TimeSpanConverter())        
        settings.NullValueHandling <- NullValueHandling.Ignore

        printfn "Url: %s%s" (client.BaseAddress.ToString()) url
        if payload.IsSome then printfn "Content: %s" <| JsonConvert.SerializeObject (payload.Value, settings)

        message.Content <- Option.fold (fun s v -> new StringContent (JsonConvert.SerializeObject (v, settings), Encoding.UTF8, "application/json")) null payload
        message.Headers.Authorization <- Option.fold (fun s token -> AuthenticationHeaderValue("Bearer", token)) null accessToken 
        async {
            return! client.SendAsync (message) |> Async.AwaitTask
        } 

    let deserializeResult<'T> (message:HttpResponseMessage) =
        let readAsString (message:HttpResponseMessage) =
            if isNull message.Content then "" else
                async {
                    let! result = message.Content.ReadAsStringAsync() |> Async.AwaitTask
                    return result
                } |> Async.RunSynchronously
        JsonConvert.DeserializeObject<'T> (readAsString message)

    type ClientType =
        | Management 
        | Graph

    let private toClient = function
        | Management -> ManagementHttpClient ()
        | Graph -> GraphClient ()

    let get<'T> ct (url:string) (accessToken:string option) =
        async { 
            let! r = send (toClient ct) HttpMethod.Get url accessToken None
            return (r.StatusCode, deserializeResult<'T> r)
        } |> Async.RunSynchronously



    let post ct (url:string) (accessToken:string option) (payload:System.Object option) =
        async { 
            let! r = send (toClient ct) HttpMethod.Post url accessToken payload
            let! content = 
                (if isNull r.Content then Task.FromResult("") else r.Content.ReadAsStringAsync()) |> Async.AwaitTask
            return (r.StatusCode, content)
        } |> Async.RunSynchronously

    let put ct (url:string) (accessToken:string option) (payload:System.Object option) =
        async { 
            let! r = send (toClient ct) HttpMethod.Put url accessToken payload
            let! content = 
                 (if isNull r.Content then Task.FromResult("") else r.Content.ReadAsStringAsync()) |> Async.AwaitTask
            return (r.StatusCode, content)
        } |> Async.RunSynchronously

    let delete ct (url:string) (accessToken:string option) =
        async { 
            let! r = send (toClient ct) HttpMethod.Delete url accessToken None
            return (r.StatusCode, "")
        } |> Async.RunSynchronously