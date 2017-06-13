open Suave
open Suave.Successful
open Suave.Operators
open System.Net
open System.IO

[<EntryPoint>]
let main argv =

    // use IIS related filter functions
    let path st = Suave.IIS.Filters.path argv st
    let pathScan format = Suave.IIS.Filters.pathScan argv format
    let pathStarts st = Suave.IIS.Filters.pathStarts argv st


    let length = argv |> Array.length |> string
    let loc = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
    // routes
    let webpart =
        choose [
            pathStarts "/st" >=> OK "Path starts with '/st'"
            path "/TextFile.txt" >=> Files.browseFileHome "TextFile.txt"
            path "/test" >=> OK "Look ma! Routing on sub-app on localhost"
            path "/" >=> OK "Hello from Suave on IIS"
            argv |> Array.map string |> Array.fold (fun acc item -> acc + "|" + item + "|") length |> OK // just debugg for incoming arguments
        ]

    // start service server
    let config = { defaultConfig with bindings=[HttpBinding.create HTTP IPAddress.Any 8083us]; } |> Suave.IIS.Configuration.withPort argv
    File.WriteAllText("c:/Temp/Suave.txt", config.homeFolder |> string)
    File.WriteAllText("c:/Temp/SuaveLoc.txt", loc |> string)
    startWebServer config webpart
    0