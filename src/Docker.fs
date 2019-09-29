namespace Bluefin

open Core

module Docker =    
    let build file image =

        execProcessString 
            "docker" 
            [   "build"
                "-f"
                file
                "-t"
                image
                "."]
            (fun o -> { o with DisplayName = "Docker" }) 
        |> debugfn "%s"

    let push image = 
        execProcessString 
            "docker" 
            [   "push"
                image]
            (fun o -> { o with DisplayName = "Docker" }) 
        |> debugfn "%s"   

                            