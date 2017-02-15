open Suave
open Suave.Successful
open Suave.Operators
open System.Net

[<EntryPoint>]
let main argv =

    // use IIS related filter functions
    let path st = Suave.IIS.Filters.path argv st
    let pathScan format = Suave.IIS.Filters.pathScan argv format

    let length = argv |> Array.length |> string

    // routes
    let webpart =
        choose [
            path "/test" >=> OK "Look ma! Routing on sub-app on localhost"
            path "/" >=> OK "Hello from Suave on IIS"
            argv |> Array.map string |> Array.fold (fun acc item -> acc + "|" + item + "|") length |> OK // just debugg for incoming arguments
        ]

    // start service server
    let config = { defaultConfig with bindings=[HttpBinding.create HTTP IPAddress.Any 8083us]; } |> Suave.IIS.Configuration.withPort argv
    startWebServer config webpart
    0