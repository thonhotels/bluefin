namespace Bluefin

open Core

module Docker =    
    let build file image =
        execProcess 
            "docker" 
            [   "build"
                "-f"
                file
                "-t"
                image
                "."]
            (fun o -> { o with DisplayName = "Docker" }) 
        |> ignore   
    let push image = 
        execProcess 
            "docker" 
            [   "push"
                image]
            (fun o -> { o with DisplayName = "Docker" }) 
        |> ignore    

                            