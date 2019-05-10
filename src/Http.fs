namespace Bluefin

open System.Net.Http
open Newtonsoft.Json
open System.Text
open System.Net.Http.Headers
open Newtonsoft.Json.Converters
open System.Threading.Tasks
open Core

module Http = 

    let send (method:HttpMethod) (url:string) (accessToken:string option) (payload:System.Object option)  =
        let message = new HttpRequestMessage (method, url)

        let settings = JsonSerializerSettings()
        settings.Converters.Add(StringEnumConverter())

        printfn "Url: %s%s" ((ManagementHttpClient ()).BaseAddress.ToString()) url
        printfn "Content: %s" <| JsonConvert.SerializeObject (payload.Value, settings)

        message.Content <- Option.fold (fun s v -> new StringContent (JsonConvert.SerializeObject (v, settings), Encoding.UTF8, "application/json")) null payload
        message.Headers.Authorization <- Option.fold (fun s token -> AuthenticationHeaderValue("Bearer", token)) null accessToken 
        async {
            return! (ManagementHttpClient ()).SendAsync (message) |> Async.AwaitTask
        } 

    let deserializeResult<'T> (message:HttpResponseMessage) =
        let readAsString (message:HttpResponseMessage) =
            if isNull message.Content then "" else
                async {
                    let! result = message.Content.ReadAsStringAsync() |> Async.AwaitTask
                    return result
                } |> Async.RunSynchronously
        JsonConvert.DeserializeObject<'T> (readAsString message)

    let get (url:string) (accessToken:string option) =
        async { 
            let! r = send HttpMethod.Get url accessToken None
            return (r.StatusCode, deserializeResult r)
        } |> Async.RunSynchronously

    let post (url:string) (accessToken:string option) (payload:System.Object option) =
        async { 
            let! r = send HttpMethod.Post url accessToken payload
            let! content = 
                (if isNull r.Content then Task.FromResult("") else r.Content.ReadAsStringAsync()) |> Async.AwaitTask
            return (r.StatusCode, content)
        } |> Async.RunSynchronously

    let put (url:string) (accessToken:string option) (payload:System.Object option) =
        async { 
            let! r = send HttpMethod.Put url accessToken payload
            let! content = 
                 (if isNull r.Content then Task.FromResult("") else r.Content.ReadAsStringAsync()) |> Async.AwaitTask
            return (r.StatusCode, content)
        } |> Async.RunSynchronously